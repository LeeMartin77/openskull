# OpenSkull

I love the card game [Skull](https://www.youtube.com/watch?v=Lu_IgiU4lh8). This is an attempt to make an open source, web based version of this game.

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
  - Place another card face-down on *top* of the cards they have already played
    - The turn ends, and it's now the next player's turn to choose beyween playing or bidding
  - Bidding a certain number
    - If the player has no cards in hand, they *must* bid
- Once bidding starts, players no longer place cards in front of them. Instead:
  - A player can bid a higher number, up to and including the number of cards on the table
  - Choose to abstain from bidding
    - Players may not re-enter bidding once they abstain
- Once either the total number of cards on the table has been bid, or all other players than the highest bid have abstained, then the player who had the highest bid *must* flip cards face up on the table until either the number they bid is revealed, or they reveal a *skull*.
  - The player must start with their own cards before flipping other player's cards
  - After their own cards, other players cards may be revealed in any order (ie. the bidder can move between players for each card)
- If the player succeeds in revealing cards, they get a point
  - If the player gets two points, they win
- If the player fails, they lose a card at random from their hand
  - If the player revealed their own skull, they instead choose the card they lose

## The Tech

The way the application should work:

- User joins a room (basically a guid). When there are more than 3 players in a room, players can vote to start a game.
- User can just join a queue of random players. 
  - When there are at least 3 players, a game forms after 10 seconds.
  - If there are 6 players, a game forms immediately for them
  - If player(s) are waiting 20 seconds, they get a suitable number of "AI" players to play against.
    - These could literally play randomly to start.


### Web Client

The web client is to be a react app.
- No signin
- Localstorage for userid/secret

### API

The API is to be a Test-driven C# API using dotnet 6

- Stateless
- Functional for game logic
- Sufficiently abstract data storage to allow for different ways of storing data

# TODO:
- API Needs to move queues/websocket messaging out of memory (kafka? rabbitmq?)
  - This sounds casual but is intense, probably needs adding a message handler to the API, and then maybe pull it to it's own service?
- Should add user secrets + display names
- Should add "Rooms" for allowing known users to start games together