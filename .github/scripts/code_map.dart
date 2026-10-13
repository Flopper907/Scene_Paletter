// Generates a compact, token-cheap code map per Dart/Flutter unit (every folder with a
// pubspec.yaml at depth 1, or under packages/). Output is meant to be read by an agent during
// plan research instead of opening files blindly — one line per file, public declarations,
// internal imports, fan-in, and whether a matching test exists.
//
//   dart .github/scripts/code_map.dart            (run from the repo root)
//
// Writes .agent-memory/code-map/units.md and .agent-memory/code-map/<unit>/map.md.
// Regenerated output, gitignored — never hand-edit. No dependencies beyond the Dart SDK.
import 'dart:io';

const _skipDirs = {'.dart_tool', 'build', '.git', '.widget_preview', 'android', 'ios', 'macos', 'linux', 'windows', 'web'};
final _decl = RegExp(
  r'^(?:(?:abstract|final|base|sealed|interface)\s+)*(class|mixin class|mixin|enum|extension type|typedef)\s+([A-Za-z]\w*)',
);
final _ext = RegExp(r'^extension\s+([A-Za-z]\w*)\s+on\b');
final _fn = RegExp(r'^(?:Future<[^>]*>|Stream<[^>]*>|void|bool|String|int|double|num|Widget|[A-Z]\w*(?:<[^>]*>)?\??)\s+([a-z]\w*)\s*\(');
final _import = RegExp(r'''^(?:import|export)\s+['"]([^'"]+)['"]''');

void main() {
  final root = Directory.current;
  final units = <Directory>[
    ...root.listSync().whereType<Directory>(),
    if (Directory('packages').existsSync()) ...Directory('packages').listSync().whereType<Directory>(),
  ].where((d) => File('${d.path}/pubspec.yaml').existsSync() && Directory('${d.path}/lib').existsSync()).toList()
    ..sort((a, b) => a.path.compareTo(b.path));

  final outRoot = Directory('.agent-memory/code-map')..createSync(recursive: true);
  final today = DateTime.now().toIso8601String().substring(0, 10);
  final index = StringBuffer()
    ..writeln('# Code-map units — generated $today by `.github/scripts/code_map.dart`. Regenerate, never edit.')
    ..writeln()
    ..writeln('| Unit | Root | Kind | Path deps | Lint preset | lib files / lines | test files | Map |')
    ..writeln('|---|---|---|---|---|---|---|---|');

  for (final unit in units) {
    final rel = _rel(unit.path, root.path);
    final pubspec = File('${unit.path}/pubspec.yaml').readAsStringSync();
    final name = RegExp(r'^name:\s*(\S+)', multiLine: true).firstMatch(pubspec)!.group(1)!;
    final pathDeps = RegExp(r'^\s+path:\s*(\.{1,2}/\S+)', multiLine: true)
        .allMatches(pubspec)
        .map((m) => m.group(1)!.split('/').last)
        .where((p) => !p.startsWith('assets'))
        .toList();
    final analysis = File('${unit.path}/analysis_options.yaml');
    final lint = !analysis.existsSync()
        ? '—'
        : analysis.readAsStringSync().contains('very_good_analysis')
            ? 'very_good_analysis'
            : 'flutter_lints';
    final isApp = File('${unit.path}/lib/main.dart').existsSync();

    final lib = _dartFiles(Directory('${unit.path}/lib'));
    final tests = [
      ..._dartFiles(Directory('${unit.path}/test')),
      ..._dartFiles(Directory('${unit.path}/integration_test')),
    ];
    final testPaths = tests.map((f) => _rel(f.path, unit.path)).toSet();

    final files = <String, _FileInfo>{};
    for (final f in lib) {
      final path = _rel(f.path, '${unit.path}/lib');
      files[path] = _parse(f, path, name);
    }
    for (final info in files.values) {
      for (final dep in info.internal) {
        files[dep]?.fanIn++;
      }
    }

    final totalLines = files.values.fold<int>(0, (s, f) => s + f.lines);
    final slug = name.replaceAll('_', '-');
    final map = StringBuffer()
      ..writeln('# $name — code map ($today, generated; regenerate with `dart .github/scripts/code_map.dart`)')
      ..writeln()
      ..writeln('Root `$rel/` · ${files.length} lib files · $totalLines lines · ${tests.length} test files'
          '${pathDeps.isEmpty ? '' : ' · path deps: ${pathDeps.join(', ')}'}')
      ..writeln()
      ..writeln('Legend: `file (lines) [T]=has test — PublicTypes; fns() → internal imports | ext: packages`')
      ..writeln()
      ..writeln('## Highest fan-in (change these carefully)')
      ..writeln();
    final byFanIn = files.values.where((f) => f.fanIn > 0).toList()..sort((a, b) => b.fanIn.compareTo(a.fanIn));
    for (final f in byFanIn.take(12)) {
      map.writeln('- ${f.path} ← ${f.fanIn}');
    }

    String? currentDir;
    for (final path in files.keys.toList()..sort()) {
      final f = files[path]!;
      final dir = path.contains('/') ? path.substring(0, path.lastIndexOf('/')) : '.';
      if (dir != currentDir) {
        map
          ..writeln()
          ..writeln('## $dir')
          ..writeln();
        currentDir = dir;
      }
      final base = path.substring(path.lastIndexOf('/') + 1);
      final hasTest = testPaths.any((t) => t.endsWith(base.replaceFirst('.dart', '_test.dart')));
      final parts = <String>[
        if (f.decls.isNotEmpty) f.decls.join(', '),
        if (f.fns.isNotEmpty) f.fns.map((n) => '$n()').join(', '),
      ];
      final deps = f.internal.map((d) => d.replaceFirst('.dart', '')).toList();
      map.writeln('- $base (${f.lines})${hasTest ? ' [T]' : ''}'
          '${parts.isEmpty ? '' : ' — ${parts.join('; ')}'}'
          '${deps.isEmpty ? '' : ' → ${deps.join(', ')}'}'
          '${f.external.isEmpty ? '' : ' | ext: ${f.external.join(', ')}'}');
    }

    Directory('${outRoot.path}/$slug').createSync(recursive: true);
    File('${outRoot.path}/$slug/map.md').writeAsStringSync(map.toString());
    index.writeln('| $name | `$rel/` | ${isApp ? 'app' : 'package'} | ${pathDeps.isEmpty ? '—' : pathDeps.join(', ')} '
        '| $lint | ${files.length} / $totalLines | ${tests.length} | `$slug/map.md` |');
  }

  index
    ..writeln()
    ..writeln('Units share nothing at code level except the path deps listed. Scope research, lint and tests to one unit.');
  File('${outRoot.path}/units.md').writeAsStringSync(index.toString());
  stdout.writeln('code-map: ${units.length} units written to ${outRoot.path}');
}

class _FileInfo {
  _FileInfo(this.path, this.lines, this.decls, this.fns, this.internal, this.external);
  final String path;
  final int lines;
  final List<String> decls;
  final List<String> fns;
  final List<String> internal;
  final List<String> external;
  int fanIn = 0;
}

_FileInfo _parse(File file, String path, String package) {
  final lines = file.readAsLinesSync();
  final decls = <String>[];
  final fns = <String>[];
  final internal = <String>{};
  final external = <String>{};
  final dir = path.contains('/') ? path.substring(0, path.lastIndexOf('/')) : '';
  for (final line in lines) {
    final d = _decl.firstMatch(line);
    if (d != null) {
      decls.add(d.group(2)!);
      continue;
    }
    final e = _ext.firstMatch(line);
    if (e != null) {
      decls.add(e.group(1)!);
      continue;
    }
    final fn = _fn.firstMatch(line);
    if (fn != null && fn.group(1) != 'main') fns.add(fn.group(1)!);
    final imp = _import.firstMatch(line);
    if (imp == null) continue;
    final uri = imp.group(1)!;
    if (uri.startsWith('dart:')) continue;
    if (uri.startsWith('package:$package/')) {
      internal.add(uri.substring('package:$package/'.length));
    } else if (uri.startsWith('package:')) {
      final pkg = uri.substring(8, uri.indexOf('/'));
      if (pkg != 'flutter') external.add(pkg);
    } else {
      internal.add(_normalize(dir.isEmpty ? uri : '$dir/$uri'));
    }
  }
  return _FileInfo(path, lines.length, decls, fns, internal.toList()..sort(), external.toList()..sort());
}

String _normalize(String path) {
  final out = <String>[];
  for (final seg in path.split('/')) {
    if (seg == '..') {
      if (out.isNotEmpty) out.removeLast();
    } else if (seg != '.' && seg.isNotEmpty) {
      out.add(seg);
    }
  }
  return out.join('/');
}

List<File> _dartFiles(Directory dir) {
  if (!dir.existsSync()) return [];
  final result = <File>[];
  void walk(Directory d) {
    for (final e in d.listSync()) {
      final name = e.uri.pathSegments.where((s) => s.isNotEmpty).last;
      if (e is Directory && !_skipDirs.contains(name) && name != 'generated') walk(e);
      if (e is File && name.endsWith('.dart') && !name.endsWith('.g.dart') && !name.endsWith('.mocks.dart')) result.add(e);
    }
  }

  walk(dir);
  return result;
}

String _rel(String path, String base) {
  final p = path.replaceAll(r'\', '/');
  final b = base.replaceAll(r'\', '/');
  final r = p.startsWith(b) ? p.substring(b.length) : p;
  return r.replaceFirst(RegExp('^/+'), '');
}
