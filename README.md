# Voxera - Cloud VoIP PBX & Call Center Platform

[![License: Commercial](https://img.shields.io/badge/License-Commercial-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-blue.svg)](https://reactjs.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://docker.com/)

## Overview

Voxera is a commercial-grade, multi-tenant, cloud-based VoIP PBX and call center platform built for SMBs, call centers, software companies, CRM providers, and technical service firms.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        VOXERA PLATFORM                          │
├──────────────┬──────────────┬──────────────┬────────────────────┤
│  React Web   │  WPF Desktop │  Mobile App  │   REST API / WS    │
│    Panel     │    Panel     │  (Future)    │   (ASP.NET Core)   │
├──────────────┴──────────────┴──────────────┴────────────────────┤
│                    ASP.NET Core Web API                         │
│              (Clean Architecture + CQRS + MediatR)              │
├──────────────┬──────────────┬──────────────┬────────────────────┤
│  PostgreSQL  │    Redis     │  RabbitMQ    │   FreeSWITCH       │
│  (Primary)   │   (Cache)    │  (Messages)  │   + Kamailio       │
└──────────────┴──────────────┴──────────────┴────────────────────┘
```

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core 8 |
| Desktop App | WPF / WinUI 3 (C#) |
| Frontend | React 18 + TypeScript |
| VoIP Engine | FreeSWITCH |
| SIP Proxy | Kamailio |
| Database | PostgreSQL 16 |
| Cache | Redis 7 |
| Message Queue | RabbitMQ 3 |
| Container | Docker + Kubernetes |
| Reverse Proxy | Nginx |

## Features

### Core PBX
- SIP user registration & management
- Internal extension system
- Call routing & transfer
- Call hold & park
- Voicemail system
- Call recording & archiving
- Call history & CDR

### IVR System
- Voice greeting
- Menu system
- Time-based routing
- DTMF handling

### Queue System
- Call queuing
- Agent round-robin
- Hold music
- Agent status management

### Call Center
- Incoming call popup
- Quick answer
- Customer screen (CRM integration)
- Real-time monitoring

### API & Integrations
- Full REST API
- Webhook system
- JWT + API Key authentication
- Rate limiting
- CRM integrations (HubSpot, Salesforce)
- Turkish providers (Netgsm, Bulutfon)
- Accounting (E-Invoice)

### AI Features
- Call summary
- Speech-to-text
- Sentiment analysis
- Auto ticket creation

### Security
- HTTPS / SRTP
- Fail2Ban integration
- IP whitelist
- DDoS protection
- Audit logging

## Development Phases

| Phase | Features |
|-------|---------|
| Phase 1 (MVP) | SIP registration, internal calls, basic panel |
| Phase 2 | API system, call recordings, web panel |
| Phase 3 | Call center, reporting, mobile support |
| Phase 4 | AI modules, multi-tenant, marketplace |

## Quick Start

```bash
# Clone repository
git clone https://github.com/your-org/voxera.git
cd voxera

# Start all services
docker-compose up -d

# Apply database migrations
docker-compose exec api dotnet ef database update

# Access services
# Web Panel:  http://localhost:3000
# API:        http://localhost:5000
# Swagger:    http://localhost:5000/swagger
# RabbitMQ:   http://localhost:15672
```

## Project Structure

```
voxera/
├── src/
│   ├── Voxera.Domain/          # Domain entities, interfaces, events
│   ├── Voxera.Application/     # CQRS commands/queries, DTOs, services
│   ├── Voxera.Infrastructure/  # EF Core, repositories, external services
│   ├── Voxera.API/             # ASP.NET Core Web API
│   ├── Voxera.Worker/          # Background jobs, message consumers
│   ├── Voxera.WebPanel/        # React frontend
│   └── Voxera.Desktop/         # WPF desktop application
├── deploy/
│   ├── docker/                 # Docker configurations
│   ├── kubernetes/             # K8s manifests
│   ├── nginx/                  # Nginx configs
│   ├── freeswitch/             # FreeSWITCH configs
│   └── kamailio/               # Kamailio configs
├── scripts/                    # DB, deploy, backup scripts
├── tests/                      # Unit & integration tests
└── docs/                       # Documentation
```

## License

Commercial License - All rights reserved. Contact sales@voxera.io for licensing.
