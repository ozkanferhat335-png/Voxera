-- ============================================================
-- Voxera Database Schema - Initial Migration
-- PostgreSQL 16+
-- ============================================================

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";  -- For full-text search

-- ============================================================
-- COMPANIES
-- ============================================================
CREATE TABLE companies (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    domain VARCHAR(255),
    logo_url VARCHAR(500),
    phone VARCHAR(50),
    email VARCHAR(255),
    address VARCHAR(500),
    tax_number VARCHAR(50),
    status INTEGER NOT NULL DEFAULT 1,  -- 1=Active, 2=Suspended, 3=Cancelled
    plan INTEGER NOT NULL DEFAULT 1,    -- 0=Trial, 1=Starter, 2=Business, 3=Enterprise
    plan_expires_at TIMESTAMPTZ,
    max_extensions INTEGER NOT NULL DEFAULT 10,
    max_concurrent_calls INTEGER NOT NULL DEFAULT 5,
    is_trial_active BOOLEAN NOT NULL DEFAULT TRUE,
    trial_ends_at TIMESTAMPTZ,
    sip_domain VARCHAR(255),
    webhook_url VARCHAR(500),
    webhook_secret VARCHAR(255),
    time_zone VARCHAR(100) DEFAULT 'Europe/Istanbul',
    country VARCHAR(10) DEFAULT 'TR',
    currency VARCHAR(10) DEFAULT 'TRY',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_companies_slug ON companies(slug);
CREATE INDEX idx_companies_status ON companies(status);

-- ============================================================
-- USERS
-- ============================================================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(500) NOT NULL,
    phone_number VARCHAR(50),
    role INTEGER NOT NULL DEFAULT 3,    -- 1=SuperAdmin, 2=CompanyAdmin, 3=Operator, 4=Accounting, 5=TechSupport
    status INTEGER NOT NULL DEFAULT 1,  -- 1=Active, 2=Inactive, 3=Suspended
    avatar_url VARCHAR(500),
    refresh_token VARCHAR(500),
    refresh_token_expires_at TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    last_login_ip VARCHAR(50),
    two_factor_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    two_factor_secret VARCHAR(100),
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    locked_until TIMESTAMPTZ,
    password_reset_token VARCHAR(500),
    password_reset_token_expires_at TIMESTAMPTZ,
    time_zone VARCHAR(100) DEFAULT 'Europe/Istanbul',
    language VARCHAR(10) DEFAULT 'tr',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_users_company_id ON users(company_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);

-- ============================================================
-- EXTENSIONS
-- ============================================================
CREATE TABLE extensions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    user_id UUID REFERENCES users(id),
    number VARCHAR(20) NOT NULL,
    display_name VARCHAR(200) NOT NULL,
    description VARCHAR(500),
    type INTEGER NOT NULL DEFAULT 1,    -- 1=User, 2=Queue, 3=IVR, 4=Conference, 5=Voicemail, 6=Trunk
    status INTEGER NOT NULL DEFAULT 1,  -- 1=Active, 2=Inactive, 3=Busy
    voicemail_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    voicemail_pin VARCHAR(10),
    forward_to VARCHAR(50),
    forward_condition INTEGER NOT NULL DEFAULT 0,  -- 0=None, 1=Always, 2=Busy, 3=NoAnswer, 4=Offline
    max_call_duration INTEGER,
    record_calls BOOLEAN NOT NULL DEFAULT FALSE,
    caller_id_name VARCHAR(100),
    caller_id_number VARCHAR(50),
    ring_timeout INTEGER NOT NULL DEFAULT 30,
    do_not_disturb BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    UNIQUE(company_id, number)
);

CREATE INDEX idx_extensions_company_id ON extensions(company_id);
CREATE INDEX idx_extensions_number ON extensions(number);

-- ============================================================
-- SIP ACCOUNTS
-- ============================================================
CREATE TABLE sip_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    extension_id UUID NOT NULL UNIQUE REFERENCES extensions(id),
    username VARCHAR(100) NOT NULL,
    password_hash VARCHAR(500) NOT NULL,
    domain VARCHAR(255) NOT NULL,
    contact_uri VARCHAR(500),
    user_agent VARCHAR(500),
    status INTEGER NOT NULL DEFAULT 1,       -- 1=Active, 2=Inactive, 3=Suspended
    agent_status INTEGER NOT NULL DEFAULT 0, -- 0=Offline, 1=Available, 2=Busy, 3=Away, 4=DND, 5=WrapUp
    last_registered_at TIMESTAMPTZ,
    last_registered_ip VARCHAR(50),
    last_registered_port INTEGER,
    transport VARCHAR(10) DEFAULT 'UDP',
    webrtc_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    nat_ip VARCHAR(50),
    max_sessions INTEGER NOT NULL DEFAULT 2,
    current_sessions INTEGER NOT NULL DEFAULT 0,
    codec VARCHAR(200) DEFAULT 'PCMU,PCMA,G729,opus',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    UNIQUE(username, domain)
);

CREATE INDEX idx_sip_accounts_company_id ON sip_accounts(company_id);
CREATE INDEX idx_sip_accounts_username ON sip_accounts(username);
CREATE INDEX idx_sip_accounts_agent_status ON sip_accounts(agent_status);

-- ============================================================
-- CALL LOGS (CDR - Call Detail Records)
-- ============================================================
CREATE TABLE call_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    call_id VARCHAR(100) NOT NULL UNIQUE,  -- FreeSWITCH UUID
    caller_number VARCHAR(50),
    caller_name VARCHAR(200),
    callee_number VARCHAR(50),
    callee_name VARCHAR(200),
    from_extension_id UUID REFERENCES extensions(id),
    to_extension_id UUID REFERENCES extensions(id),
    direction INTEGER NOT NULL,  -- 1=Inbound, 2=Outbound, 3=Internal
    status INTEGER NOT NULL DEFAULT 1,  -- 1=Initiated, 2=Ringing, 3=Active, 4=OnHold, 5=Completed, 6=Missed, 7=Failed
    end_reason INTEGER,  -- 1=Normal, 2=NoAnswer, 3=Busy, 4=Failed, 5=Cancelled, 6=Transferred, 7=Timeout
    started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    answered_at TIMESTAMPTZ,
    ended_at TIMESTAMPTZ,
    duration_seconds INTEGER,
    billable_seconds INTEGER,
    ring_duration_seconds INTEGER,
    is_recorded BOOLEAN NOT NULL DEFAULT FALSE,
    recording_path VARCHAR(500),
    recording_size BIGINT,
    sip_trunk_id VARCHAR(100),
    ivr_path VARCHAR(500),
    queue_id UUID,
    notes TEXT,
    tags VARCHAR(500),
    is_transferred BOOLEAN NOT NULL DEFAULT FALSE,
    transferred_to VARCHAR(50),
    cost DECIMAL(10,4),
    ai_summary TEXT,
    transcript TEXT,
    sentiment INTEGER,  -- 1=Positive, 2=Neutral, 3=Negative
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_call_logs_company_started ON call_logs(company_id, started_at DESC);
CREATE INDEX idx_call_logs_company_status ON call_logs(company_id, status);
CREATE INDEX idx_call_logs_caller_number ON call_logs(caller_number);
CREATE INDEX idx_call_logs_call_id ON call_logs(call_id);
CREATE INDEX idx_call_logs_direction ON call_logs(direction);

-- ============================================================
-- API KEYS
-- ============================================================
CREATE TABLE api_keys (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    user_id UUID REFERENCES users(id),
    name VARCHAR(200) NOT NULL,
    key_hash VARCHAR(500) NOT NULL UNIQUE,
    key_prefix VARCHAR(20) NOT NULL,
    status INTEGER NOT NULL DEFAULT 1,  -- 1=Active, 2=Revoked, 3=Expired
    permissions TEXT,  -- Comma-separated: calls:read,calls:write,sip:manage
    ip_whitelist VARCHAR(1000),
    expires_at TIMESTAMPTZ,
    last_used_at TIMESTAMPTZ,
    last_used_ip VARCHAR(50),
    request_count BIGINT NOT NULL DEFAULT 0,
    rate_limit_per_minute INTEGER NOT NULL DEFAULT 60,
    rate_limit_per_day INTEGER NOT NULL DEFAULT 10000,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_api_keys_company_id ON api_keys(company_id);
CREATE INDEX idx_api_keys_key_hash ON api_keys(key_hash);

-- ============================================================
-- IVR MENUS
-- ============================================================
CREATE TABLE ivr_menus (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    name VARCHAR(200) NOT NULL,
    description VARCHAR(500),
    greeting_audio_path VARCHAR(500),
    greeting_text TEXT,
    timeout INTEGER NOT NULL DEFAULT 5,
    max_retries INTEGER NOT NULL DEFAULT 3,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    time_zone VARCHAR(100) DEFAULT 'Europe/Istanbul',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE ivr_options (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ivr_menu_id UUID NOT NULL REFERENCES ivr_menus(id) ON DELETE CASCADE,
    digit VARCHAR(2) NOT NULL,  -- 0-9, *, #
    description VARCHAR(200) NOT NULL,
    action_type INTEGER NOT NULL,  -- 1=Extension, 2=Queue, 3=IvrMenu, 4=Voicemail, 5=Hangup, 6=External, 7=Announcement
    action_target VARCHAR(200),
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE ivr_schedules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ivr_menu_id UUID NOT NULL REFERENCES ivr_menus(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    active_days TEXT,  -- JSON array of DayOfWeek
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    action_type INTEGER NOT NULL,
    action_target VARCHAR(200),
    is_holiday BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

-- ============================================================
-- CALL QUEUES
-- ============================================================
CREATE TABLE call_queues (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    name VARCHAR(200) NOT NULL,
    description VARCHAR(500),
    extension VARCHAR(20) NOT NULL,
    strategy INTEGER NOT NULL DEFAULT 1,  -- 1=RoundRobin, 2=LeastRecent, 3=FewestCalls, 4=Random, 5=Priority
    max_wait_time INTEGER NOT NULL DEFAULT 300,
    max_queue_size INTEGER NOT NULL DEFAULT 50,
    music_on_hold_path VARCHAR(500),
    welcome_audio_path VARCHAR(500),
    announce_interval INTEGER NOT NULL DEFAULT 60,
    announce_position BOOLEAN NOT NULL DEFAULT TRUE,
    announce_wait_time BOOLEAN NOT NULL DEFAULT TRUE,
    overflow_target VARCHAR(200),
    overflow_action INTEGER NOT NULL DEFAULT 1,  -- 1=Voicemail, 2=Extension, 3=IvrMenu, 4=Hangup, 5=External
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE queue_agents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    queue_id UUID NOT NULL REFERENCES call_queues(id) ON DELETE CASCADE,
    extension_id UUID NOT NULL REFERENCES extensions(id),
    priority INTEGER NOT NULL DEFAULT 1,
    penalty INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    wrap_up_time INTEGER NOT NULL DEFAULT 10,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    UNIQUE(queue_id, extension_id)
);

-- ============================================================
-- INVOICES
-- ============================================================
CREATE TABLE invoices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id),
    invoice_number VARCHAR(50) NOT NULL UNIQUE,
    type INTEGER NOT NULL,  -- 1=Subscription, 2=Usage, 3=OneTime, 4=Credit
    status INTEGER NOT NULL DEFAULT 1,  -- 1=Draft, 2=Sent, 3=Paid, 4=Overdue, 5=Cancelled
    issued_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    due_at TIMESTAMPTZ NOT NULL,
    paid_at TIMESTAMPTZ,
    sub_total DECIMAL(12,2) NOT NULL DEFAULT 0,
    tax_rate DECIMAL(5,4) NOT NULL DEFAULT 0.20,
    tax_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    total DECIMAL(12,2) NOT NULL DEFAULT 0,
    currency VARCHAR(10) NOT NULL DEFAULT 'TRY',
    notes TEXT,
    payment_method VARCHAR(100),
    payment_reference VARCHAR(200),
    pdf_path VARCHAR(500),
    plan INTEGER,
    period_months INTEGER,
    period_start TIMESTAMPTZ,
    period_end TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE TABLE invoice_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    invoice_id UUID NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    description VARCHAR(500) NOT NULL,
    quantity DECIMAL(10,2) NOT NULL,
    unit_price DECIMAL(12,4) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

-- ============================================================
-- AUDIT LOGS
-- ============================================================
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID REFERENCES companies(id),
    user_id UUID REFERENCES users(id),
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id VARCHAR(100),
    old_values JSONB,
    new_values JSONB,
    ip_address VARCHAR(50),
    user_agent VARCHAR(500),
    is_success BOOLEAN NOT NULL DEFAULT TRUE,
    error_message TEXT,
    additional_data JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_audit_logs_company_id ON audit_logs(company_id);
CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_created_at ON audit_logs(created_at DESC);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);

-- ============================================================
-- SEED DATA - Super Admin
-- ============================================================
INSERT INTO companies (id, name, slug, status, plan, sip_domain, time_zone)
VALUES ('00000000-0000-0000-0000-000000000001', 'Voxera System', 'voxera-system', 1, 3, 'system.sip.voxera.io', 'Europe/Istanbul');

-- Password: Admin@123456 (bcrypt hash)
INSERT INTO users (id, company_id, first_name, last_name, email, password_hash, role, status)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001',
    'Super',
    'Admin',
    'admin@voxera.io',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.s5uO.e',
    1,  -- SuperAdmin
    1   -- Active
);
