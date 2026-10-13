// Audits one app against the clean-arch-feature-scaffold rules (layer rule, plugin placement, 300-line limit,
// get_it outside the composition root, imperative navigation). Read-only; mirrors notes_app/test/architecture_test.dart.
//   dart .github/scripts/arch_check.dart <app_dir> <package_name>     e.g. ... pdfs_app pdfs_app
import 'dart:io';

const layers = ['domain', 'application', 'infrastructure', 'presentation'];
const pure = {
  'dart:async',
  'dart:convert',
  'dart:core',
  'dart:math',
  'dart:collection',
  'package:equatable/equatable.dart',
  'package:llm_kit/llm_kit.dart',
  'package:meta/meta.dart',
};
const notPlugins = {
  'app_ui',
  'flutter',
  'flutter_localizations',
  'flutter_bloc',
  'bloc',
  'equatable',
  'markdown_editor_interpreter',
  'intl',
  'get_it',
  'go_router',
  'dropdown_button2',
  'introduction_screen',
  'collection',
  'meta',
};
String layer(String p) => RegExp('^lib/(domain|application|infrastructure|presentation)/').firstMatch(p)?.group(1) ?? 'root';
void main(List<String> args) {
  final app = args[0];
  final pkg = args[1];
  final lib = Directory('$app/lib');
  final v = <String, int>{};
  final examples = <String, String>{};
  var long = <String>[];
  var getit = 0;
  var nav = 0;
  var total = 0;
  final roots = <String>{};
  for (final f in lib.listSync(recursive: true).whereType<File>().where((f) => f.path.endsWith('.dart'))) {
    final p = 'lib/${f.path.replaceAll(r'\', '/').split('/lib/').last}';
    if (p.contains('/generated/')) continue;
    total++;
    final l = layer(p);
    if (l == 'root') roots.add(p.split('/').take(2).join('/'));
    final text = f.readAsStringSync();
    final n = text.split('\n').length;
    if (n > 300) long.add('${p.substring(4)}($n)');
    if (text.contains("package:$pkg/injection.dart") &&
        !['lib/injection.dart', 'lib/main.dart', 'lib/presentation/app.dart', 'lib/app.dart'].contains(p))
      getit++;
    if (p.startsWith('lib/presentation/') && RegExp(r'Navigator\.(of\(context\)\.)?push|MaterialPageRoute').hasMatch(text)) nav++;
    for (final m in RegExp("^import '([^']+)'", multiLine: true).allMatches(text)) {
      final imp = m.group(1)!;
      final target = imp.startsWith('package:$pkg/') ? layer('lib/${imp.substring(pkg.length + 9)}') : null;
      final isPlugin =
          imp.startsWith('package:') &&
          !pure.contains(imp) &&
          !notPlugins.contains(imp.substring(8).split('/').first) &&
          target == null &&
          !imp.startsWith('package:$pkg/');
      String? why;
      switch (l) {
        case 'domain':
          why = target != null
              ? (target == 'domain' ? null : 'domain→$target')
              : (pure.contains(imp) || imp.startsWith('dart:') && !imp.startsWith('dart:io') && !imp.startsWith('dart:ui')
                    ? null
                    : 'domain impure');
          break;
        case 'application':
          why = target != null
              ? (target == 'domain' || target == 'application' ? null : 'application→$target')
              : (pure.contains(imp) ||
                        imp.startsWith('dart:') && imp != 'dart:io' ||
                        imp.startsWith('package:flutter_bloc') ||
                        imp.startsWith('package:bloc/')
                    ? null
                    : 'application impure');
          break;
        case 'infrastructure':
          why = target == 'presentation' || target == 'application' ? 'infrastructure→$target' : null;
          break;
        case 'presentation':
          why = target == 'infrastructure' ? 'presentation→infrastructure' : (isPlugin ? 'plugin in presentation' : null);
          break;
      }
      if (why != null) {
        v[why] = (v[why] ?? 0) + 1;
        examples.putIfAbsent(why, () => '${p.substring(4)} → $imp');
      }
    }
  }
  print('== $app: $total files | non-layer roots: ${roots.isEmpty ? '-' : roots.join(', ')}');
  print('   violations: ${v.isEmpty ? 'none' : v.entries.map((e) => '${e.key}×${e.value}').join(', ')}');
  examples.forEach((k, e) => print('     e.g. $k: $e'));
  print('   >300 lines: ${long.length} ${long.take(6).join(', ')}');
  print('   get_it outside root: $getit | imperative nav in presentation: $nav');
}
