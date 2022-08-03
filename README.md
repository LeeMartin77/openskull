# [OpenSkull](https://www.openskull.dev)

![API](https://github.com/leemartin77/openskull/actions/workflows/api-dotnet-build-and-test.yml/badge.svg)
![API Publish](https://github.com/leemartin77/openskull/actions/workflows/api-docker-publish.yml/badge.svg)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/leemartin77/openskull)](https://github.com/LeeMartin77/openskull/releases/latest)
[![Docker Pulls](https://img.shields.io/docker/pulls/leemartin77/openskull-api)](https://hub.docker.com/r/leemartin77/openskull-api)

![WebApp](https://github.com/leemartin77/openskull/actions/workflows/webapp-build-and-push.yml/badge.svg)

I love the card game [Skull](https://www.youtube.com/watch?v=Lu_IgiU4lh8). This is an open source, web based version of this game.

## The Game

### Setup

- Each player is dealt 4 cards
  - 3 "Flower" cards, 1 "Skull" card

### Rules of play

- Starting from a player, everyone places one card face-down in front of them
  - The starting player is either:
    - random on the first round
    - The winner of the last round
    - The player whose skull was revealed causing the bidder to lose
- Once every player has placed a card in front of them, play runs sequentially in a loop.
- On a turn, a player may:
  - Place another card face-down on _top_ of the cards they have already played
    - The turn ends, and it's now the next player's turn to choose beyween playing or bidding
  - Bidding a certain number
    - If the player has no cards in hand, they _must_ bid
- Once bidding starts, players no longer place cards in front of them. Instead:
  - A player can bid a higher number, up to and including the number of cards on the table
  - Choose to abstain from bidding
    - Players may not re-enter bidding once they abstain
- Once either the total number of cards on the table has been bid, or all other players than the highest bid have abstained, then the player who had the highest bid _must_ flip cards face up on the table until either the number they bid is revealed, or they reveal a _skull_.
  - The player must start with their own cards before flipping other player's cards
  - After their own cards, other players cards may be revealed in any order (ie. the bidder can move between players for each card)
- If the player succeeds in revealing cards, they get a point
  - If the player gets two points, they win
- If the player fails, they lose a card at random from their hand
  - If the player revealed their own skull, they instead choose the card they lose

## The Tech

The way the application should work:

- User joins a room. When there are more than 3 players in a room, players can vote to start a game.
- User can just join a queue of random players.
  - When there are enough players, a game forms after 10 seconds.
  - If there are 6 players, a game forms immediately for them

### Web Client

The web client is a Svelte SPA app

- No signin
- Localstorage for userid, secret and nickname

### API

The API is a Test-driven C# API using dotnet 6

- Stateless
- Functional code for game logic
- Sufficiently abstract data storage to allow for different ways of storing data
- Tests primarily around the game logic, with lighter tests for player wrangling
