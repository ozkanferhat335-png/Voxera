# Voxera Architecture Documentation

## System Overview

```
                    ┌─────────────────────────────────────────────────────┐
                    │                   INTERNET                          │
                    └──────────────────────┬──────────────────────────────┘
                                           │
                    ┌──────────────────────▼──────────────────────────────┐
                    │              Nginx Reverse Proxy                    │
                    │         (SSL Termination, Rate Limiting)            │
                    └──────┬───────────────────────────────┬──────────────┘
                           │                               │
              ┌────────────▼──────────┐      ┌────────────▼──────────────┐
              │   React Web Panel     │      │   ASP.NET Core API        │
              │   (Port 3000/80)      │      │   (Port 5000)             │
              │                       │      │   - REST API              │
              │   - Dashboard         │      │   - SignalR Hubs          │
              │   - Call Logs         │      │   - JWT Auth              │
              │   - Extensions        │      │   - API Key Auth          │
              │   - IVR Config        │      │   - Rate Limiting         │
              │   - Reports           │      │   - Swagger UI            │
              └───────────────────────┘      └────────────┬──────────────┘
                                                          │
              ┌───────────────────────────────────────────┼──────────────┐
              │                    Data Layer             │              │
              │                                           │              │
              │  ┌──────────────┐  ┌──────────────┐  ┌──▼───────────┐  │
              │  │  PostgreSQL  │  │    Redis     │  │  RabbitMQ    │  │
              │  │  (Primary)   │  │   (Cache)    │  │  (Messages)  │  │
              │  │              │  │              │  │              │  │
              │  │  - Users     │  │  - Sessions  │  │  - Call Evts │  │
              │  │  - Companies │  │  - Dashboard │  │  - Webhooks  │  │
              │  │  - CallLogs  │  │  - Rate Lmt  │  │  - AI Queue  │  │
              │  │  - SipAccts  │  │  - Agent Sts │  │              │  │
              │  └──────────────┘  └──────────────┘  └──────────────┘  │
              └───────────────────────────────────────────────────────────┘
                                           │
              ┌────────────────────────────▼──────────────────────────────┐
              │                    VoIP Layer                             │
              │                                                           │
              │  ┌──────────────────────┐  ┌──────────────────────────┐  │
              │  │     Kamailio         │  │     FreeSWITCH           │  │
              │  │   (SIP Proxy)        │  │   (Media Server)         │  │
              │  │                      │  │                          │  │
              │  │  - SIP Registration  │  │  - Call Control          │  │
              │  │  - Load Balancing    │  │  - RTP Media             │  │
              │  │  - Authentication    │  │  - Recording             │  │
              │  │  - NAT Traversal     │  │  - IVR/Dialplan          │  │
              │  │  - DDoS Protection   │  │  - WebRTC Gateway        │  │
              │  │  - Rate Limiting     │  │  - SRTP Encryption       │  │
              │  └──────────────────────┘  └──────────────────────────┘  │
              └───────────────────────────────────────────────────────────┘
```

## Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    Voxera.API                               │
│  (Controllers, Middleware, Hubs, Filters)                   │
├─────────────────────────────────────────────────────────────┤
│                 Voxera.Application                          │
│  (Commands, Queries, DTOs, Interfaces, Validators)          │
│  CQRS Pattern with MediatR                                  │
├─────────────────────────────────────────────────────────────┤
│                  Voxera.Domain                              │
│  (Entities, Enums, Domain Events, Value Objects)            │
│  Pure C# - No external dependencies                         │
├─────────────────────────────────────────────────────────────┤
│               Voxera.Infrastructure                         │
│  (EF Core, Redis, RabbitMQ, FreeSWITCH, AI, Webhooks)      │
└─────────────────────────────────────────────────────────────┘
```

## Multi-Tenant Architecture

Each company (tenant) has:
- Isolated data via `company_id` foreign keys
- Separate SIP domain: `{slug}.sip.voxera.io`
- Independent extension numbering
- Separate API keys and webhooks
- Plan-based limits (extensions, concurrent calls)

## Call Flow

```
Inbound Call:
PSTN → SIP Trunk → Kamailio → FreeSWITCH → IVR/Queue → Extension

Outbound Call:
Extension → FreeSWITCH → Kamailio → SIP Trunk → PSTN

Internal Call:
Extension A → FreeSWITCH → Extension B
```

## Security Architecture

1. **API Security**: JWT + API Key dual authentication
2. **SIP Security**: Kamailio authentication + SRTP encryption
3. **Network**: Nginx SSL termination + rate limiting
4. **Data**: PostgreSQL row-level security via company_id
5. **Audit**: All write operations logged to audit_logs table
6. **DDoS**: Kamailio Pike module + Nginx rate limiting
7. **Secrets**: Environment variables, never in code

## Scaling Strategy

- **API**: Horizontal scaling via Kubernetes HPA (2-10 pods)
- **Database**: PostgreSQL with read replicas
- **Cache**: Redis Cluster
- **VoIP**: Multiple FreeSWITCH nodes behind Kamailio
- **Storage**: Shared NFS/S3 for recordings
