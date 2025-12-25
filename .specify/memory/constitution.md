<!--
===============================================================================
SYNC IMPACT REPORT
===============================================================================
Version change: 0.0.0 → 1.0.0 (MAJOR: Initial constitution creation)

Modified principles:
  - N/A (initial creation)

Added sections:
  - Core Principles (3 principles: Simplicity First, Production Quality,
    Educational Clarity)
  - Technology Stack (Section 2)
  - Deployment Strategy (Section 3)
  - Governance

Removed sections:
  - N/A (initial creation)

Templates requiring updates:
  - .specify/templates/plan-template.md: ✅ No updates required (generic)
  - .specify/templates/spec-template.md: ✅ No updates required (generic)
  - .specify/templates/tasks-template.md: ✅ No updates required (generic)

Follow-up TODOs:
  - None
===============================================================================
-->

# MQ Playground Constitution

## Core Principles

### I. Simplicity First

All features MUST start with the simplest possible implementation that meets
requirements. Over-engineering and speculative abstractions are prohibited.

**Rules**:
- YAGNI (You Aren't Gonna Need It): Do not implement features "just in case"
- Avoid premature optimization; measure before optimizing
- Prefer explicit code over clever abstractions
- A working simple solution is better than an elegant incomplete one

**Rationale**: This project is a learning playground. Complex architectures
obscure the messaging patterns we aim to demonstrate.

### II. Production Quality

Code MUST be maintainable, documented where non-obvious, and suitable for
review by others.

**Rules**:
- All public APIs MUST have XML documentation comments in C# code
- Vue components MUST have prop/emit documentation
- Error handling MUST be explicit; silent failures are prohibited
- Logging MUST be present for debugging MQ message flows
- Code MUST follow established naming conventions for the stack

**Rationale**: Even experimental code benefits from good practices. This
ensures code can be revisited and understood weeks or months later.

### III. Educational Clarity

Code MUST prioritize understandability for learning purposes. Messaging
patterns (producers, consumers, pub/sub) MUST be clearly separated and
documented.

**Rules**:
- Separation of concerns: producers, consumers, and shared models MUST be in
  distinct namespaces/projects
- Comments MUST explain the "why" of messaging patterns, not just the "what"
- MQ configuration MUST be externalized and documented
- SignalR hubs MUST clearly indicate which MQ events they relay
- Sample messages and scenarios MUST be documented in code or README files

**Rationale**: The primary purpose of MQ Playground is to demonstrate and
teach IBM MQ integration patterns. Clarity serves this educational mission.

## Technology Stack

This section documents the approved technology stack for MQ Playground.

**Backend**:
- .NET 8 (C#)
- ASP.NET Core Web API for REST endpoints
- Worker Services for background MQ consumers
- SignalR for real-time browser updates

**Frontend**:
- Vue.js 2 (Options API)
- Standard Vue CLI or Vite tooling

**Messaging**:
- IBM MQ via Docker container
- IBM.XMS.NETCore or amqmdnet for .NET MQ client

**Infrastructure**:
- Docker Compose for local development orchestration
- Environment-based configuration (appsettings.json, .env files)

**Testing** (when applicable):
- xUnit for .NET unit tests
- Vue Test Utils for component testing

## Deployment Strategy

All deployment MUST follow these guidelines to ensure consistency and
reproducibility.

**Local Development**:
- Docker Compose MUST be the primary method for running all services locally
- A single `docker-compose up` command SHOULD bring up API, frontend, and MQ
- Environment variables MUST be documented in a sample `.env.example` file

**Configuration Management**:
- Secrets MUST NOT be committed to source control
- MQ connection details MUST be configurable via environment variables
- appsettings.{Environment}.json files MAY be used for non-sensitive settings

**Build Artifacts**:
- .NET projects MUST produce Docker images via multi-stage Dockerfiles
- Vue.js frontend MUST produce static assets served via nginx or similar

## Governance

This constitution is the authoritative source for project practices. All
contributions MUST comply with these principles.

**Amendment Process**:
1. Propose changes via pull request to this file
2. Changes MUST include rationale and impact assessment
3. Version MUST be incremented according to semantic versioning:
   - MAJOR: Principle removed or fundamentally redefined
   - MINOR: New principle or section added
   - PATCH: Clarifications, wording fixes
4. LAST_AMENDED_DATE MUST be updated to the merge date

**Compliance**:
- All code reviews SHOULD verify adherence to Core Principles
- Deviations MUST be documented and justified in code comments or PR
  descriptions
- Complexity beyond "Simplicity First" MUST have explicit justification

**Version**: 1.0.0 | **Ratified**: 2025-12-26 | **Last Amended**: 2025-12-26
