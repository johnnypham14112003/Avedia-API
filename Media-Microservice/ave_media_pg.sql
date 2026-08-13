-- DROP DATABASE IF EXISTS avedia_media WITH (FORCE);

-- CREATE DATABASE avedia_media;

-- SHOW TIMEZONE;

-- DROP SCHEMA public CASCADE;
-- CREATE SCHEMA public;

-- Create COLLATION for "string" ignoring case
CREATE COLLATION IF NOT EXISTS case_insensitive (
    provider = icu,
    locale = 'und-u-ks-level1',
    deterministic = false
);
--======================[ MEDIA SERVICE ]======================
CREATE TABLE IF NOT EXISTS actors (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	full_name TEXT NOT NULL COLLATE "case_insensitive",
	stage_name TEXT COLLATE "case_insensitive",
	gender BOOLEAN,
	dob DATE,
	bio TEXT,
	height VARCHAR(5) COLLATE "case_insensitive",
	cup_size VARCHAR(5) COLLATE "case_insensitive",
	"size" VARCHAR(20) COLLATE "case_insensitive",
	debut_date DATE,
	nationality VARCHAR(100) COLLATE "case_insensitive",
	company VARCHAR(200) COLLATE "case_insensitive",
	status VARCHAR(30) NOT NULL DEFAULT 'Created' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS videos (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	code VARCHAR(100) COLLATE "case_insensitive",
	title TEXT NOT NULL COLLATE "case_insensitive",
	original_title TEXT NOT NULL COLLATE "case_insensitive",
	description TEXT,
	duration_minutes INTEGER NOT NULL DEFAULT 0,
	series VARCHAR(200) COLLATE "case_insensitive",
	episode SMALLINT NOT NULL DEFAULT 1,
	director TEXT,
	release_date DATE,
	"language" VARCHAR(100) COLLATE "case_insensitive",
	status VARCHAR(30) NOT NULL DEFAULT 'Created' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS genres (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL UNIQUE COLLATE "case_insensitive",
	description TEXT,
	status VARCHAR(30) NOT NULL DEFAULT 'Created' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS producers (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	"name" VARCHAR(200) NOT NULL UNIQUE COLLATE "case_insensitive",
	other_name VARCHAR(200),
	description TEXT,
	establish_date DATE, --as company
	country VARCHAR(100) COLLATE "case_insensitive",
	status VARCHAR(30) NOT NULL DEFAULT 'Created' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS labels (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL UNIQUE COLLATE "case_insensitive",
	description TEXT,
	status VARCHAR(30) NOT NULL DEFAULT 'Available' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS tags (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	title VARCHAR(50) NOT NULL UNIQUE COLLATE "case_insensitive", --eg: 'Tag Name'
	slug VARCHAR(50) NOT NULL UNIQUE COLLATE "case_insensitive", --eg: 'tag-name'
	status VARCHAR(30) NOT NULL DEFAULT 'Available' COLLATE "case_insensitive"
);

CREATE TABLE IF NOT EXISTS images (
	"id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
	locale_path TEXT,
	url TEXT NOT NULL,
	upload_date DATE NOT NULL DEFAULT CURRENT_DATE,
	status VARCHAR(30) NOT NULL DEFAULT 'Created' COLLATE "case_insensitive"
);

--======================[ N:N RELATIONSHIP ]======================
CREATE TABLE IF NOT EXISTS video_genres (
	video_id UUID REFERENCES videos ("id") ON DELETE CASCADE,
	genre_id UUID REFERENCES genres ("id") ON DELETE CASCADE,
	PRIMARY KEY (video_id, genre_id)
);

CREATE TABLE IF NOT EXISTS video_producers (
	video_id UUID REFERENCES videos ("id") ON DELETE CASCADE,
	producer_id UUID REFERENCES producers ("id") ON DELETE CASCADE,
	PRIMARY KEY (video_id, producer_id)
);

CREATE TABLE IF NOT EXISTS video_labels (
	video_id UUID REFERENCES videos ("id") ON DELETE CASCADE,
	label_id UUID REFERENCES labels ("id") ON DELETE CASCADE,
	PRIMARY KEY (video_id, label_id)
);

CREATE TABLE IF NOT EXISTS video_tags (
	video_id UUID REFERENCES videos ("id") ON DELETE CASCADE,
	tag_id UUID REFERENCES tags ("id") ON DELETE CASCADE,
	PRIMARY KEY (video_id, tag_id)
);

-- 1 video have many actors, 1 actor film many videos
CREATE TABLE video_actors (
	video_id UUID REFERENCES videos ("id") ON DELETE CASCADE,
	actor_id UUID REFERENCES actors ("id") ON DELETE CASCADE,
	PRIMARY KEY (video_id, actor_id),
	role_main BOOLEAN NOT NULL DEFAULT FALSE,
	"status" VARCHAR(30) COLLATE "case_insensitive"
);

-- 1 video have many images
CREATE TABLE IF NOT EXISTS video_images (
	video_id UUID REFERENCES videos ("id") ON DELETE CASCADE,
	image_id UUID REFERENCES images ("id") ON DELETE CASCADE,
	PRIMARY KEY ("video_id", "image_id"),
	is_main BOOLEAN NOT NULL DEFAULT FALSE,
	order_numerical SMALLINT NOT NULL DEFAULT 1,
	"status" VARCHAR(30) COLLATE "case_insensitive"
);

-- 1 actor have many images, 1 images include many actors
CREATE TABLE IF NOT EXISTS actor_images (
	actor_id UUID REFERENCES actors ("id") ON DELETE CASCADE,
	image_id UUID REFERENCES images ("id") ON DELETE CASCADE,
	PRIMARY KEY (actor_id, image_id),
	is_cover BOOLEAN NOT NULL DEFAULT FALSE,
	is_avartar BOOLEAN NOT NULL DEFAULT FALSE,
	order_numerical SMALLINT NOT NULL DEFAULT 1,
	"status" VARCHAR(30) COLLATE "case_insensitive"
);

--======================[ INDEXING ]======================
-- 1. B-Tree Index for cho filter fields
CREATE INDEX IF NOT EXISTS idx_videos_code ON videos (code);
CREATE INDEX IF NOT EXISTS idx_videos_status ON videos (status);
CREATE INDEX IF NOT EXISTS idx_actors_status ON actors (status);

-- 2. B-Tree Index for FK (N:N)
-- Video <-> Actors
CREATE INDEX IF NOT EXISTS idx_video_actors_video_id ON video_actors (video_id);
CREATE INDEX IF NOT EXISTS idx_video_actors_actor_id ON video_actors (actor_id);

-- Video <-> Genres / Tags / Labels / Producers
CREATE INDEX IF NOT EXISTS idx_video_genres_video_id ON video_genres (video_id);
CREATE INDEX IF NOT EXISTS idx_video_genres_genre_id ON video_genres (genre_id);

CREATE INDEX IF NOT EXISTS idx_video_tags_video_id ON video_tags (video_id);
CREATE INDEX IF NOT EXISTS idx_video_tags_tag_id ON video_tags (tag_id);

CREATE INDEX IF NOT EXISTS idx_video_labels_video_id ON video_labels (video_id);
CREATE INDEX IF NOT EXISTS idx_video_labels_label_id ON video_labels (label_id);

CREATE INDEX IF NOT EXISTS idx_video_producers_video_id ON video_producers (video_id);
CREATE INDEX IF NOT EXISTS idx_video_producers_producer_id ON video_producers (producer_id);

-- Images Mapping
CREATE INDEX IF NOT EXISTS idx_video_images_video_id ON video_images (video_id);
CREATE INDEX IF NOT EXISTS idx_video_images_image_id ON video_images (image_id);

CREATE INDEX IF NOT EXISTS idx_actor_images_actor_id ON actor_images (actor_id);
CREATE INDEX IF NOT EXISTS idx_actor_images_image_id ON actor_images (image_id);