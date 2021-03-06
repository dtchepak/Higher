﻿namespace Higher.Core

// Continuation Monad type
type Cont<'R, 'T> = C of (('T -> 'R) -> 'R)
type Cont private () =
    static let token = new Cont()
    static member Inj (value : Cont<'R, 'T>) : App2<Cont, 'R, 'T> = 
        let app = new App<Cont, 'R>(token, value)
        new App2<Cont, 'R, 'T>(AppToken<Cont, 'R>.Token token, app)
    static member Prj (app2 : App2<Cont, 'R, 'T>) : Cont<'R, 'T> = 
        let app = app2.Apply(AppToken<Cont, 'R>.Token token) :?> App<Cont, 'R>
        app.Apply(token) :?> _

// Continuation Monad instance
type ContMonad<'R>() = 
    inherit Monad<App<Cont, 'R>>() with
    override self.Return x = Cont.Inj <| C (fun k -> k x)
    override self.Bind (m, f) = 
        Cont.Inj <| C (fun k -> 
                            let (C contF) = Cont.Prj m
                            contF (fun x -> 
                                        let (C contF') = Cont.Prj <| f x
                                        contF' k))

