namespace calc

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Templating

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        let mutable (onum:double), (num:double), op = 0., 0., None
        let mutable strNum = ""
        let mutable decimals:int = 0

        // input field definition
        let display = input [attr.``type`` "text"; attr.value "0."; attr.readonly ""] []

        // *******************************************************************
        // updateDisplayByStr() -- show the string value on the display field.
        // *******************************************************************
        let updateDisplayByStr (str:string) =
            let mutable s, sNum = "", ""
            // no decimal calculation, no adding decimal point.
            match str with
            | "E" ->
                s <- str
                sNum <- "0."
            | "" | "-" ->
                s <- "0."
                sNum <- "0."
            | _ ->
                s <- str
                sNum <- str
            if not (String.exists (fun x -> x = '.') s) then s <- s + "."
            decimals <- s.Length-1 - s.IndexOf(".")
            let display = display :?> Elt
            display.Value <- s
            num <- sNum |> double

        // *******************************************************************
        // updateDisplay() -- show the number value on the display field.
        // *******************************************************************
        let updateDisplay () =
            // supporting "divide by zero", "decimal point", "decimal adjustment", "round"
            if (num = -infinity) || (num = infinity) || (string num) ="NaN" then
                updateDisplayByStr ("E")
            else
                num <- round(num * 10. ** 10.) / (10. ** 10.)     // round the decimal
                strNum <- string num
                if not (String.exists (fun x -> x = '.') strNum) then strNum <- strNum + "."
                decimals <- strNum.Length-1 - strNum.IndexOf(".")
                let display = display :?> Elt
                display.Value <- strNum

        // *******************************************************************
        // C() -- onclick for [C] button; Clear
        // *******************************************************************                
        let C () =
            num  <- 0.; decimals <- 0
            updateDisplay()

        // *******************************************************************
        // AC() -- onclick for [AC] button; All Clear
        // *******************************************************************                
        let AC () =
            num  <- 0.; decimals <- 0
            onum <- 0.
            op   <- None
            updateDisplay ()

        // *******************************************************************
        // N() -- onclick for [+/-] button; Negative
        // *******************************************************************                
        let N () =
            num <- - num; decimals <- 0
            updateDisplay ()

        // *******************************************************************
        // PI() -- input for [𝝅(pi)] button; Pi (3.14.....)
        // onclick                
        let PI () =
            num <- Math.PI
            updateDisplay ()

        // *******************************************************************
        // SIN() -- onclick for [Sin] button;
        // *******************************************************************                
        let SIN () =
            num <- Math.Sin(num)
            updateDisplay ()

        // *******************************************************************
        // COS() -- onclick for [Cos] button;
        // *******************************************************************                
        let COS () =
            num <- Math.Cos(num)
            updateDisplay ()

        // *******************************************************************
        // TAN() -- onclick for [Tan] button;
        // *******************************************************************                
        let TAN () =
            num <- Math.Tan(num)
            updateDisplay ()

        // *******************************************************************
        // BS() -- onclick for [<X] button; Backspace
        // *******************************************************************                
        let BS () =
            strNum <- strNum.[0..(String.length strNum-(if strNum.[strNum.Length-1]='.' then 3 else 2))]
            updateDisplayByStr (strNum)

        // *******************************************************************
        // P() -- onclick for [.] button; Decimal Point
        // *******************************************************************                
        let P () =
            if decimals = 0 then
                updateDisplay ()
                decimals <- 1

        // *******************************************************************
        // I() -- onclick for [1/x] button; Inverse
        // *******************************************************************                
        let I () =
            num <- 1. / num
            updateDisplay ()

        // *******************************************************************
        // E() -- onclick for [=] button; Equal
        // *******************************************************************                
        let E () =
            match op with
            | None ->
                ()
            | Some f ->
                num <- f onum num
                updateDisplay ()
                op  <- None

        // *******************************************************************
        // O() -- onclick for [/*-+] button; calculate / * + -
        // *******************************************************************                
        let O o () =
            match op with
            | None ->
                ()
            | Some f ->
                num <- f onum num
                updateDisplay ()
            onum <- num
            num  <- 0.; decimals <- 0
            op   <- Some o

        // *******************************************************************
        // btn() -- onclick for all buttons.  caption and class for the button
        // *******************************************************************                
        let btn caption cl action =
            button [attr.``class`` cl; on.click (fun _ _ -> action ())] [text caption]

        // *******************************************************************
        // D() -- display for the digits 0 1 2 3 4 5 6 7 8 9 buttons
        // *******************************************************************                
        let D n =
            if decimals = 0 then
                num <- 10. * num + n
                updateDisplay ()
            else
                strNum <- strNum + (string n)
                updateDisplayByStr (strNum)

        // *******************************************************************
        // digit() -- onclick for 0 1 2 3 4 5 6 7 8 9 buttons.
        // *******************************************************************                
        let digit n =
            btn (string n) "D" (fun () -> D n)

        // *******************************************************************
        // calculator -- key layout
        // *******************************************************************                
        let calculator =
            div [] [
                display
                br [] []
                div [attr.style "padding: 10px 0px;"] [
                    btn "𝝅"  "PI" PI ; btn "Sin" "SIN" SIN; btn "Cos" "COS" COS ; btn "Tan" "TAN" TAN  ; btn "⌫" "BS" BS       ; br [] []
                    btn "+/-" "N"  N  ; digit    7.        ; digit     8.        ; digit    9.          ; btn "÷" "DIV" (O ( / )); br [] []
                    btn "1/𝑥" "I"  I  ; digit    4.        ; digit     5.        ; digit    6.          ; btn "×" "MUL" (O ( * )); br [] []
                    btn "C"   "C"  C  ; digit    1.        ; digit     2.        ; digit    3.          ; btn "-" "SUB" (O ( - )); br [] []
                    btn "AC"  "AC" AC ; digit    0.        ; btn "."  "P"  P     ; btn "=" "E"  E       ; btn "+" "PLS" (O ( + ))
                ]
            ]

        IndexTemplate.Main()
            .Main(
                calculator
                )
            .Doc()
        |> Doc.RunById "main"
