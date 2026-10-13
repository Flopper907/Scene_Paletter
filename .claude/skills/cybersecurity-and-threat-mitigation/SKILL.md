---
name: cybersecurity-and-threat-mitigation
description: Applies secure coding standards, OWASP mobile security guidelines, threat mitigation, data encryption, and network hardening strategies.
---

# Skill: Cybersecurity & Threat Mitigation

## Description
A complete security skill covering secure coding standards, common threat mitigations (OWASP Mobile/Web Top 10), data protection, and architectural security patterns.

## Instructions
When writing, reviewing, or configuring code for security-critical tasks:

1. **Data Protection & Secure Storage**:
   - **Never store sensitive data in plaintext**: Use secure storage abstractions (`flutter_secure_storage`, KeyStore on Android, Keychain on iOS).
   - **Memory & Storage Sanitation**: Keep API tokens, secret keys, and PII out of shared preferences, unencrypted databases, logs, or static variables.
   - **Environment Separation**: Store secrets and base URLs in `.env` or build flavors (`--dart-define`), never in source code.

2. **Network & API Security**:
   - Enforce HTTPS/TLS everywhere. Implement SSL/TLS Certificate Pinning for high-security endpoints.
   - Sanitize all user inputs before sending requests or rendering to prevent Injection attacks, XSS, and SQL Injection.
   - Implement proper authentication header lifecycle (OAuth2 with secure token refresh, automatic session expiration).

3. **Client-Side & Runtime Integrity**:
   - **Code Obfuscation**: Always build release versions with obfuscation enabled (`--obfuscate --split-debug-info`).
   - Prevent sensitive screens from being screenshotted or showing up in background task switchers (`FLAG_SECURE`).
   - Implement root/jailbreak detection and integrity checks where appropriate.

## Output Requirements
- Secure implementation patterns, vulnerability callouts, and hardened code snippets.