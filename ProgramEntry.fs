// Basic SFML with F#
// https://mfranc.com/blog/game-programming-with-f-c-and-sfml-gameloop/
//
// Game of life adapted from this sniped + playing around with what goes in F#
// http://www.fssnip.net/cU/title/Game-of-Life

open System
open SFML.Graphics
open SFML.Window
open SFML.System
open System.Diagnostics

type Field = | Dead = 0 | Alive = 1
let gameWidth, gameHeight = 800, 450
let fieldSize = 2

let getValue x y (board:Field[,]) =
    let x = (x + gameWidth) % gameWidth
    let y = (y + gameHeight) % gameHeight
    board.[x, y]

let draw (window:RenderWindow) (board:Field[,]) =
    let rect = new RectangleShape ()
    rect.Size <- new Vector2f (float32 fieldSize, float32 fieldSize)
    rect.FillColor <- Color.White
    board |> Array2D.iteri (fun x y v ->
        if v.Equals Field.Alive then
            rect.Position <- new Vector2f (float32 (x * fieldSize), float32 (y * fieldSize))
            window.Draw rect
    )

let computeNumNeighbours x y (board:Field[,]) =
    [ for dx in -1 .. 1 do
        for dy in -1 .. 1 do
            if dx <> 0 || dy <> 0 then
                yield getValue (x + dx) (y + dy) board]
    |> Seq.map int
    |> Seq.sum

let gameStep (board:Field[,]) =
    board |> Array2D.mapi (fun x y v ->
        match computeNumNeighbours x y board with
        | 3 -> Field.Alive
        | 2 -> v
        | _ -> Field.Dead )

[<EntryPoint>]
let main argv = 
    let mainWindow = new RenderWindow(new VideoMode (uint32 fieldSize * uint32 gameWidth, uint32 fieldSize * uint32 gameHeight), "GameOfLife")
    mainWindow.Closed.AddHandler(fun sender args -> (sender :?> RenderWindow).Close())

    let stopwatch = new Stopwatch ()
    stopwatch.Start ()

    let updatedBoard (board:Field[,]) =
        if stopwatch.ElapsedMilliseconds > 100L then
            stopwatch.Restart ()
            gameStep board
        else
            board

    let rec mainLoop (board:Field[,]) = 
        mainWindow.Clear ()
        mainWindow.DispatchEvents ()
        draw mainWindow board
        mainWindow.Display ()
        match mainWindow.IsOpen with
        | true -> updatedBoard board |> mainLoop |> ignore
        | false -> ()
    
    let rnd = new Random()
    let board = Array2D.init gameWidth gameHeight (fun _ _ -> enum<Field> (rnd.Next(2)) )
    mainLoop board
    0