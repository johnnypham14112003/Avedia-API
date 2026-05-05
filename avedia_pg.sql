-- DROP DATABASE IF EXISTS avedia WITH (FORCE);

-- CREATE DATABASE avedia;

-- SHOW TIMEZONE;

-- DROP SCHEMA public CASCADE;
-- CREATE SCHEMA public;
--======================[ PRIMARY TABLE ]======================
CREATE TABLE IF NOT EXISTS accounts (
	"id" UUID PRIMARY KEY DEFAULT GEN_RANDOM_UUID(),
	avatar_url TEXT,
	jwt_session TEXT,
	refresh_token TEXT,
	refresh_token_expirytime TIMESTAMP(0),
	user_name VARCHAR(50) UNIQUE NOT NULL,
	email VARCHAR(200) UNIQUE NOT NULL,
	is_verified BOOLEAN NOT NULL DEFAULT FALSE,
	password_hash TEXT NOT NULL,
	gender BOOLEAN,
	nationality VARCHAR(100),
	joined_date DATE NOT NULL DEFAULT CURRENT_DATE,
	merit_point INTEGER NOT NULL DEFAULT 0,
	"role" VARCHAR(30) NOT NULL DEFAULT 'User',
	status VARCHAR(30) NOT NULL DEFAULT 'Available'
);

CREATE TABLE IF NOT EXISTS badges (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL,
	description VARCHAR(200),
	icon_url TEXT,
	locale_path TEXT,
	rare_level SMALLINT NOT NULL DEFAULT 1, --easy -> hard
	created_date DATE NOT NULL DEFAULT CURRENT_DATE,
	status VARCHAR(30) NOT NULL DEFAULT 'Available'
);

CREATE TABLE IF NOT EXISTS notifications (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	"type" VARCHAR(30),
	type_id UUID,
	title VARCHAR(50) NOT NULL,
	"message" TEXT,
	is_global BOOLEAN NOT NULL DEFAULT FALSE,
	created_date DATE NOT NULL DEFAULT CURRENT_DATE
);

CREATE TABLE IF NOT EXISTS favorites (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	lover_id UUID REFERENCES accounts ("id") NOT NULL,
	target_type VARCHAR(30) NOT NULL,
	target_id UUID
);

CREATE TABLE IF NOT EXISTS contributions (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	contributor_id UUID REFERENCES accounts ("id") NOT NULL,
	approver_id UUID REFERENCES accounts ("id"),
	admin_reviewed BOOLEAN NOT NULL DEFAULT FALSE,
	target_type VARCHAR(30) NOT NULL,
	target_id UUID,
	action_type VARCHAR(10) NOT NULL,
	requested_date DATE NOT NULL DEFAULT CURRENT_DATE,
	proposed_data JSONB,
	status VARCHAR(30) NOT NULL DEFAULT 'Pending' --'Approved', 'Rejected'
);

CREATE TABLE IF NOT EXISTS actors (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	full_name TEXT NOT NULL,
	stage_name TEXT,
	gender BOOLEAN,
	dob DATE,
	bio TEXT,
	height VARCHAR(5),
	cup_size VARCHAR(5),
	"size" VARCHAR(20),
	debut_date DATE,
	nationality VARCHAR(100),
	company VARCHAR(200),
	status VARCHAR(30)
);

CREATE TABLE IF NOT EXISTS videos (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	code VARCHAR(100),
	title TEXT NOT NULL,
	original_title TEXT,
	description TEXT,
	duration_minutes INTEGER NOT NULL DEFAULT 0,
	serie VARCHAR(200),
	episode SMALLINT NOT NULL DEFAULT 1,
	director TEXT,
	release_date DATE,
	"language" VARCHAR(100),
	status VARCHAR(30)
);

CREATE TABLE IF NOT EXISTS genres (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL UNIQUE,
	description TEXT,
	status VARCHAR(30)
);

CREATE TABLE IF NOT EXISTS producers (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	"name" VARCHAR(200) NOT NULL UNIQUE,
	other_name VARCHAR(200),
	description TEXT,
	establish_date DATE, --as company
	country VARCHAR(100),
	status VARCHAR(30)
);

CREATE TABLE IF NOT EXISTS labels (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL UNIQUE,
	description TEXT,
	status VARCHAR(30) NOT NULL DEFAULT 'Available'
);

CREATE TABLE IF NOT EXISTS tags (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL UNIQUE, --eg: 'Tag Name'
	slug VARCHAR(50) NOT NULL UNIQUE, --eg: 'tag-name'
	status VARCHAR(30) NOT NULL DEFAULT 'Available'
);

CREATE TABLE IF NOT EXISTS images (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	locale_path TEXT,
	url TEXT,
	upload_date DATE NOT NULL DEFAULT CURRENT_DATE,
	status VARCHAR(30)
);

--======================[ N:N RELATIONSHIP ]======================
CREATE TABLE IF NOT EXISTS account_badges (
	account_id UUID REFERENCES accounts ("id"),
	badge_id UUID REFERENCES badges ("id"),
	PRIMARY KEY (account_id, badge_id),
	awarded_date DATE NOT NULL DEFAULT CURRENT_DATE
);

CREATE TABLE IF NOT EXISTS account_notifications (
	account_id UUID REFERENCES accounts ("id"),
	notification_id UUID REFERENCES notifications ("id"),
	PRIMARY KEY (account_id, notification_id),
	is_read BOOLEAN NOT NULL DEFAULT FALSE,
	created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS video_genres (
	video_id UUID REFERENCES videos ("id"),
	genre_id UUID REFERENCES genres ("id"),
	PRIMARY KEY (video_id, genre_id)
);

CREATE TABLE IF NOT EXISTS video_producers (
	video_id UUID REFERENCES videos ("id"),
	producer_id UUID REFERENCES producers ("id"),
	PRIMARY KEY (video_id, producer_id)
);

CREATE TABLE IF NOT EXISTS video_labels (
	video_id UUID REFERENCES videos ("id"),
	label_id UUID REFERENCES labels ("id"),
	PRIMARY KEY (video_id, label_id)
);

CREATE TABLE IF NOT EXISTS video_tags (
	video_id UUID REFERENCES videos ("id"),
	tag_id UUID REFERENCES tags ("id"),
	PRIMARY KEY (video_id, tag_id)
);

-- 1 video have many actors, 1 actor film many videos
CREATE TABLE video_actors (
	video_id UUID REFERENCES videos ("id"),
	actor_id UUID REFERENCES actors ("id"),
	PRIMARY KEY (video_id, actor_id),
	role_main BOOLEAN NOT NULL DEFAULT FALSE,
	"status" VARCHAR(30)
);

-- 1 video have many images
CREATE TABLE IF NOT EXISTS video_images (
	video_id UUID REFERENCES videos ("id"),
	image_id UUID REFERENCES images ("id"),
	PRIMARY KEY ("video_id", "image_id"),
	is_main BOOLEAN NOT NULL DEFAULT FALSE,
	order_numerical SMALLINT NOT NULL DEFAULT 1,
	"status" VARCHAR(30)
);

-- 1 actor have many images, 1 images include many actors
CREATE TABLE IF NOT EXISTS actor_images (
	actor_id UUID REFERENCES actors ("id"),
	image_id UUID REFERENCES images ("id"),
	PRIMARY KEY (actor_id, image_id),
	is_main BOOLEAN NOT NULL DEFAULT FALSE,
	order_numerical SMALLINT NOT NULL DEFAULT 1,
	"status" VARCHAR(30)
);

--===========================[ INDEXING ]===========================
--==================================================================
-- primary key default index search left -> right
--(eg: key(video_id, actor_id) <=> search all actor by video_id <=> mean: search of left)

-- create index in reverse for search video by actor_id
CREATE INDEX idx_video_actors_actor_id ON video_actors (actor_id);

-- Help search video by genre, tag, producer, label
CREATE INDEX idx_video_genres_genre_id ON video_genres (genre_id);
CREATE INDEX idx_video_tags_tag_id ON video_tags (tag_id);
CREATE INDEX idx_video_producers_producer_id ON video_producers (producer_id);
CREATE INDEX idx_video_labels_label_id ON video_labels (label_id);

-- Help query image of a video or actor
CREATE INDEX idx_video_images_image_id ON video_images (image_id);
CREATE INDEX idx_actor_images_image_id ON actor_images (image_id);

-- <<<<<<<<<<<<<<< VIDEOS >>>>>>>>>>>>>>>
-- Query by code
CREATE INDEX idx_videos_code ON videos (code);

-- Query by title
CREATE INDEX idx_videos_title ON videos (title);
CREATE INDEX idx_videos_original_title ON videos (original_title);

-- Query in Description
CREATE INDEX idx_videos_description ON videos (description);

-- Query by series
CREATE INDEX idx_videos_series ON videos (series);

-- Query by director
CREATE INDEX idx_videos_director ON videos (director);

-- Query/Sort by release date
CREATE INDEX idx_videos_release_date ON videos (release_date);

-- Query by status
CREATE INDEX idx_videos_status ON videos (status);

-- <<<<<<<<<<<<<<< ACTORS >>>>>>>>>>>>>>>
-- Query by name and stage_name
CREATE INDEX idx_actors_stage_name ON actors (stage_name);
CREATE INDEX idx_actors_full_name ON actors (full_name);

-- Query by gender
CREATE INDEX idx_actors_gender ON actors (gender);

-- Query by height  
CREATE INDEX idx_actors_height   ON actors (height  );

-- Query by cup_size 
CREATE INDEX idx_actors_cup_size  ON actors (cup_size );

-- Query by debut_date
CREATE INDEX idx_actors_debut_date ON actors (debut_date);

-- Query by nationality 
CREATE INDEX idx_actors_nationality  ON actors (nationality );

-- Query by status
CREATE INDEX idx_actors_status ON actors (status);

-- <<<<<<<<<<<<<<< Composite Index >>>>>>>>>>>>>>>
-- Query list contribute for a video/actor
CREATE INDEX idx_contributions_target ON contributions (target_type, target_id);

-- Query list request pending for Admin
CREATE INDEX idx_contributions_pending ON contributions (requested_date) WHERE status = 'Pending';

-- Query list users favorited of a 1 video/actor
CREATE INDEX idx_favorites_target ON favorites (target_type, target_id);

-- list items favorited of a user (for user query)
CREATE INDEX idx_favorites_lover_id ON favorites (lover_id);

-- <<<<<<<<<<<<<<< System >>>>>>>>>>>>>>>
-- Query unread noti of a user
CREATE INDEX idx_account_notifications_unread ON account_notifications (account_id) WHERE is_read = FALSE;