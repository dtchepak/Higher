﻿namespace Higher.Core

// Reader Monad type
type Reader<'R, 'T> = R of ('R -> 'T)
type Reader private () =
    static let token = new Reader()
    static member Inj (value : Reader<'R, 'T>) : App2<Reader, 'R, 'T> =
        let app = new App<Reader, 'R>(token, value)
        new App2<Reader, 'R, 'T>(AppToken<Reader, 'R>.Token token, app)
    static member Prj (app2 : App2<Reader, 'R, 'T>) : Reader<'R, 'T> = 
        let app = app2.Apply(AppToken<Reader, 'R>.Token token) :?> App<Reader, 'R>
        app.Apply(token) :?> _

// Reader Monad instance
type ReaderMonad<'R>() = 
    inherit Monad<App<Reader, 'R>>() with
    override self.Return x = Reader.Inj <| R (fun env -> x)
    override self.Bind (m, f) = 
        Reader.Inj <| R (fun env -> 
                            let (R rf) = Reader.Prj m 
                            let (R rf') = Reader.Prj <| f (rf env)
                            rf' env)
    member self.Get() : App2<Reader, 'R, 'R> =
        Reader.Inj <| R (fun env -> env) 

