# Affinity Chess
A chess engine written in C# as a hobby project to learn more about C# and programming in general. The code is heavily commented for my own benefit, maybe it will also help someone else.

## General information
Affinity Chess is developed with the following basic concepts:

:small_blue_diamond: Written from scratch in C#. <br/>
:small_blue_diamond: [Bitboard](https://www.chessprogramming.org/Bitboards) representation with magic bitboards for sliding pieces. <br/>
:small_blue_diamond: Pseudo legal move generation for quiet- and capturing moves separately. <br/>
:small_blue_diamond: Make/unmake move approach. <br/>
:small_blue_diamond: Incremental [Zobrist hashing](https://www.chessprogramming.org/Zobrist_Hashing). <br/>
:small_blue_diamond: A standard [Negamax](https://www.chessprogramming.org/Negamax) framework with alpha/beta pruning for search. <br/>
:small_blue_diamond: [UCI](https://www.chessprogramming.org/UCI) protocol for communication with GUIs. <br/>

## Features
### Version 1.0
Version 1.0 has the following features implemented:

:heavy_check_mark: [Iterative deepening](https://www.chessprogramming.org/Iterative_Deepening) <br/>
:heavy_check_mark: [Principal variation search](https://www.chessprogramming.org/Principal_Variation_Search) <br/>
:heavy_check_mark: [Quiescence search](https://www.chessprogramming.org/Quiescence_Search) <br/>
:heavy_check_mark: [Delta pruning](https://www.chessprogramming.org/Delta_Pruning) (in Quiescence search) <br/>
:heavy_check_mark: [Transposition table](https://www.chessprogramming.org/Transposition_Table) <br/>
:heavy_check_mark: [Null move pruning](https://www.chessprogramming.org/Null_Move_Pruning) <br/>
:heavy_check_mark: [Mate distance pruning](https://www.chessprogramming.org/Mate_Distance_Pruning) <br/>
:heavy_check_mark: [Razoring](https://www.chessprogramming.org/Razoring) <br/>
:heavy_check_mark: [Late move reduction](https://www.chessprogramming.org/Late_Move_Reductions) <br/>
:heavy_check_mark: [Check extension](https://www.chessprogramming.org/Check_Extensions) <br/>
:heavy_check_mark: Basic evaluation function based on [PeSTO](https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function) <br/>

#### <b>Future improvements</b>
The evaluation function in version 1.0 is only based on piece square tables. Future evaluation functions will include more concepts such as pawn structure, open files and king safety. 

In the search function all moves are generated and sorted before tested in the recursive loop. Since the first few moves often leads to a cutoff there is a lot of unnecessary work done with sorting the list on before hand. Future versions will look into [staged move generation](https://www.chessprogramming.org/Move_Generation#Staged_Move_Generation) where the best moves are generated and tried first, and only if they fail are the rest of the moves generated. 

Move generation and the make/unmake move functions are working, but are relatively slow. The speed in this version is around 1,2 million [NPS](https://www.chessprogramming.org/Nodes_per_Second) during [Pertf testing](https://www.chessprogramming.org/Perft). Since these functions are crucial for the NPS speed they will be more optimized in future versions. 

Too many percent of search is now spent in quiescence search. Future versions will try to reduce this to get a better branching factor and therefore search fewer nodes. 

## How to play

## Bugs or support
Please let me know if you find any bugs with the engine, or if you need any support in how to set it up. Also let me know if there are any features you are currently missing and I will try to implement them in coming versions. 

## Acknowledgements
A lot of inspiration for the transposition tables are taken from [Leorik](https://github.com/lithander/Leorik). <br/>
Inpiration for the magic bitboard approach was taken from [Cosette](https://github.com/Tearth/Cosette) and the [YouTube series](https://www.youtube.com/watch?v=QUNP-UjujBM&list=PLmN0neTso3Jxh8ZIylk74JpwfiWNI76Cs) from [Maksim Korzh](https://www.chessprogramming.org/Maksim_Korzh). <br/>
Huge thanks also to the TalkChess community for helping me out along the way, I wouldn't have gotten this far without you.  <br/>
