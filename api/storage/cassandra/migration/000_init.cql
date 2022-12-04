CREATE KEYSPACE IF NOT EXISTS openskull WITH REPLICATION = {'class':'SimpleStrategy','replication_factor':1};
CREATE TABLE IF NOT EXISTS openskull.games
(
    id uuid,
    player_ids set<uuid>,
    version_tag text,
    game text,
    last_updated timestamp,
    PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ON openskull.games(player_ids);
CREATE INDEX IF NOT EXISTS ON openskull.games(version_tag);
CREATE INDEX IF NOT EXISTS ON openskull.games(last_updated);
CREATE TABLE IF NOT EXISTS openskull.players
(
    id uuid,
    hashed_secret blob,
    salt text,
    nickname text,
    PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ON openskull.players(hashed_secret);
CREATE INDEX IF NOT EXISTS ON openskull.players(salt);