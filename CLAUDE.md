@.github/copilot-instructions.md

## Claude Code notes
- Skills live in `.claude/skills/<name>/SKILL.md` as copies of `.github/skills/*.md`;
  `.claude/agents/critic.md` copies `.github/agents/critic.agent.md`. Edit the `.github/` source,
  copy, run `.github/scripts/check-mirrors.sh` (`.ps1` on Windows).
- `.claude/settings.json` wires the enforcement hooks (`.claude/hooks/*.sh`, bash — needs Git Bash
  or WSL on Windows). Read hook files with the Read tool; the bash guard also blocks harmless
  commands that mention a hook path together with a `>` redirect.
