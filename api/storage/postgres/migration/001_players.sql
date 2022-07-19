CREATE TABLE IF NOT EXISTS players 
(id UUID PRIMARY KEY, hashed_secret text, salt text, nickname text);