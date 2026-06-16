-- DROP DATABASE IF EXISTS avedia WITH (FORCE);

-- CREATE DATABASE avedia;

-- SHOW TIMEZONE;

-- DROP SCHEMA public CASCADE;
-- CREATE SCHEMA public;

-- Create COLLATION for "string" ignoring case
CREATE COLLATION IF NOT EXISTS case_insensitive (
    provider = icu,
    locale = 'und-u-ks-level2',
    deterministic = false
);
--======================[ USER SERVICE ]======================
CREATE TABLE IF NOT EXISTS accounts (
	"id" UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
	avatar_url TEXT,
	otp_code VARCHAR(20),
	refresh_token TEXT,
	refresh_token_expirytime TIMESTAMP(0),
	user_name VARCHAR(50) UNIQUE NOT NULL COLLATE "case_insensitive",
	email VARCHAR(200) UNIQUE NOT NULL COLLATE "case_insensitive",
	is_verified BOOLEAN NOT NULL DEFAULT FALSE,
	password_hash TEXT NOT NULL,
	gender BOOLEAN,
	nationality VARCHAR(100) COLLATE "case_insensitive",
	joined_date DATE NOT NULL DEFAULT CURRENT_DATE,
	merit_point INTEGER NOT NULL DEFAULT 0,
	"role" VARCHAR(30) NOT NULL DEFAULT 'User' COLLATE "case_insensitive",
	status VARCHAR(30) NOT NULL DEFAULT 'Available' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS badges (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL COLLATE "case_insensitive",
	description VARCHAR(200) COLLATE "case_insensitive",
	icon_url TEXT,
	locale_path TEXT,
	rare_level SMALLINT NOT NULL DEFAULT 1, --easy -> hard
	created_date DATE NOT NULL DEFAULT CURRENT_DATE,
	status VARCHAR(30) NOT NULL DEFAULT 'Available' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS notifications (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	"type" VARCHAR(30),
	type_id UUID,
	title VARCHAR(50) NOT NULL COLLATE "case_insensitive",
	"message" TEXT,
	is_global BOOLEAN NOT NULL DEFAULT FALSE,
	created_date DATE NOT NULL DEFAULT CURRENT_DATE
);

CREATE TABLE IF NOT EXISTS favorites (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	account_id UUID REFERENCES accounts ("id") ON DELETE CASCADE NOT NULL,
	target_type VARCHAR(30) NOT NULL COLLATE "case_insensitive", -- 'Video', 'Actor', 'Blog'
	target_id UUID NOT NULL -- Soft link to UUID of Video/Actor
);

CREATE TABLE IF NOT EXISTS contributions (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	contributor_id UUID REFERENCES accounts ("id") NOT NULL,
	approver_id UUID REFERENCES accounts ("id"),
	admin_approved BOOLEAN NOT NULL DEFAULT FALSE,
	target_type VARCHAR(30) NOT NULL COLLATE "case_insensitive", -- 'Video', 'Actor'
	target_id UUID, -- NULL for request create new
	action_type VARCHAR(10) NOT NULL COLLATE "case_insensitive",
	handled_date DATE,
	requested_date DATE NOT NULL DEFAULT CURRENT_DATE,
	proposed_data JSONB,
	status VARCHAR(30) NOT NULL DEFAULT 'Pending' COLLATE "case_insensitive" --'Approved', 'Rejected'
);

--======================[ N:N RELATIONSHIP ]======================
CREATE TABLE IF NOT EXISTS account_badges (
	account_id UUID REFERENCES accounts ("id") ON DELETE CASCADE,
	badge_id UUID REFERENCES badges ("id") ON DELETE CASCADE,
	PRIMARY KEY (account_id, badge_id),
	awarded_date DATE NOT NULL DEFAULT CURRENT_DATE
);

CREATE TABLE IF NOT EXISTS account_notifications (
	account_id UUID REFERENCES accounts ("id") ON DELETE CASCADE,
	notification_id UUID REFERENCES notifications ("id") ON DELETE CASCADE,
	PRIMARY KEY (account_id, notification_id),
	is_read BOOLEAN NOT NULL DEFAULT FALSE,
	created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

--======================[ INDEXING ]======================
-- 1. Activate extension for search text contains
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- 2. GIN Index for search Contains
CREATE INDEX IF NOT EXISTS idx_accounts_username_trgm ON accounts USING gin (user_name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_accounts_email_trgm ON accounts USING gin (email gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_badges_title_trgm ON badges USING gin (title gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_badges_description_trgm ON badges USING gin (description gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_notifications_title_trgm ON notifications USING gin (title gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_contributions_target_type_trgm ON contributions USING gin (target_type gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_contributions_action_type_trgm ON contributions USING gin (action_type gin_trgm_ops);

-- 3. B-Tree Index for filter fields
CREATE INDEX IF NOT EXISTS idx_accounts_nationality ON accounts (nationality);
CREATE INDEX IF NOT EXISTS idx_accounts_role ON accounts ("role");
CREATE INDEX IF NOT EXISTS idx_accounts_status ON accounts (status);
CREATE INDEX IF NOT EXISTS idx_contributions_status ON contributions (status);
CREATE INDEX IF NOT EXISTS idx_badges_rare_level ON badges (rare_level);
CREATE INDEX IF NOT EXISTS idx_badges_status ON badges (status);

-- 4. B-Tree Index for ALL FK (For JOIN / .Include())
CREATE INDEX IF NOT EXISTS idx_account_badges_account_id ON account_badges (account_id);
CREATE INDEX IF NOT EXISTS idx_account_badges_badge_id ON account_badges (badge_id);
CREATE INDEX IF NOT EXISTS idx_account_notif_account_id ON account_notifications (account_id);
CREATE INDEX IF NOT EXISTS idx_account_notif_notif_id ON account_notifications (notification_id);
CREATE INDEX IF NOT EXISTS idx_favorites_account_id ON favorites (account_id);
CREATE INDEX IF NOT EXISTS idx_favorites_target_id ON favorites (target_id);
CREATE INDEX IF NOT EXISTS idx_contributions_contributor_id ON contributions (contributor_id);
CREATE INDEX IF NOT EXISTS idx_contributions_target_id ON contributions (target_id);