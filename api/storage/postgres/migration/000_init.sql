CREATE TABLE IF NOT EXISTS games 
(id UUID PRIMARY KEY, player_ids UUID[], version_tag text, game json, last_updated timestamp);

CREATE TABLE IF NOT EXISTS players 
(id UUID PRIMARY KEY, hashed_secret bytea, salt text, nickname text);