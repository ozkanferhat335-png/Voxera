# Voxera REST API Documentation

Base URL: `https://api.voxera.io/api/v1`

## Authentication

### JWT Bearer Token
```
Authorization: Bearer <access_token>
```

### API Key
```
X-API-Key: vxr_<your_api_key>
```

---

## Auth Endpoints

### POST /auth/register
Register a new company and admin user.

**Request:**
```json
{
  "companyName": "Acme Corp",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@acme.com",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "...",
  "userId": "uuid",
  "fullName": "John Doe",
  "email": "john@acme.com",
  "role": "CompanyAdmin",
  "companyId": "uuid",
  "companyName": "Acme Corp"
}
```

### POST /auth/login
```json
{ "email": "john@acme.com", "password": "SecurePass123!" }
```

### POST /auth/refresh
```json
{ "refreshToken": "..." }
```

### GET /auth/me
Returns current user info (requires JWT).

---

## Extensions

### GET /extensions
List all extensions for the company.

### POST /extensions
```json
{
  "number": "101",
  "displayName": "John Doe",
  "type": "User"
}
```

### PUT /extensions/{id}
```json
{
  "forwardTo": "102",
  "forwardCondition": "NoAnswer",
  "doNotDisturb": false,
  "voicemailEnabled": true,
  "recordCalls": true
}
```

### DELETE /extensions/{id}

---

## SIP Accounts

### GET /sip-accounts
### POST /sip-accounts
```json
{
  "extensionId": "uuid",
  "enableWebRtc": false
}
```
Returns SIP credentials (password shown only once).

### PATCH /sip-accounts/{id}/status
```json
{ "status": "Available" }
```
Status values: `Available`, `Busy`, `Away`, `DoNotDisturb`, `WrapUp`, `Offline`

---

## Calls

### GET /calls
Query params: `page`, `pageSize`, `search`, `direction`, `status`, `from`, `to`

### POST /calls/originate
```json
{
  "fromExtension": "101",
  "toNumber": "102"
}
```

### POST /calls/{callId}/hangup
### POST /calls/{callId}/transfer
```json
{ "destination": "103" }
```

### POST /calls/{callId}/hold
```json
{ "hold": true }
```

### GET /calls/{callId}/recording
Returns signed download URL.

---

## IVR

### GET /ivr
### POST /ivr
```json
{
  "name": "Main Menu",
  "greetingText": "Welcome! Press 1 for sales, 2 for support."
}
```

### POST /ivr/{menuId}/options
```json
{
  "digit": "1",
  "description": "Sales",
  "actionType": "Queue",
  "actionTarget": "queue-uuid"
}
```
Action types: `Extension`, `Queue`, `IvrMenu`, `Voicemail`, `Hangup`, `ExternalNumber`, `Announcement`

---

## Queues

### GET /queues
### POST /queues
```json
{ "name": "Support Queue", "extension": "9001" }
```

### POST /queues/{queueId}/agents
```json
{ "extensionId": "uuid", "priority": 1 }
```

### DELETE /queues/{queueId}/agents/{agentId}

---

## Dashboard

### GET /dashboard/stats
Returns real-time statistics (cached 30s).

### GET /dashboard/active-calls

---

## Reports

### GET /reports/daily?date=2024-01-15
### GET /reports/operator-performance?from=2024-01-01&to=2024-01-31
### GET /reports/missed-calls?from=2024-01-15

---

## API Keys

### GET /api-keys
### POST /api-keys
```json
{
  "name": "CRM Integration",
  "permissions": ["calls:read", "calls:write"],
  "expiresAt": "2025-01-01T00:00:00Z"
}
```

### DELETE /api-keys/{id}

---

## Webhooks

Voxera sends POST requests to your configured webhook URL for these events:

| Event | Description |
|-------|-------------|
| `IncomingCall` | New inbound call received |
| `CallAnswered` | Call was answered |
| `CallEnded` | Call ended |
| `MissedCall` | Call was not answered |
| `RecordingReady` | Recording file is ready |
| `AgentStatusChanged` | Agent changed their status |

**Payload format:**
```json
{
  "event_type": "CallEnded",
  "company_id": "uuid",
  "timestamp": "2024-01-15T10:30:00Z",
  "data": { ... }
}
```

**Signature verification:**
```
X-Voxera-Signature: sha256=<hmac-sha256-hex>
X-Voxera-Event: CallEnded
X-Voxera-Timestamp: 1705312200
```

---

## Real-time Events (SignalR)

Connect to: `wss://api.voxera.io/hubs/calls?access_token=<jwt>`

**Events received:**
- `IncomingCall` - New call popup
- `CallEnded` - Call finished
- `AgentStatusChanged` - Agent status update

**Methods to invoke:**
- `UpdateAgentStatus(status)` - Update your agent status

---

## Rate Limits

| Endpoint | Limit |
|----------|-------|
| All endpoints | 100 req/min |
| POST /auth/login | 10 req/5min |
| API Key requests | Configurable per key |

HTTP 429 is returned when rate limit is exceeded.
