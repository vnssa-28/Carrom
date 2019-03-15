Public Class Form1
    Dim btp As New Bitmap(500, 500)
    Dim g As Graphics = Graphics.FromImage(btp)
    Dim temp As circle
    Dim listofdisks As New CList
    Dim addtarget As Boolean = False
    Public Shared friction As Double = 0.05
    Dim radius As Integer = 25
    Dim launch As Boolean = False
    Dim strikerexist As Boolean = False
    Dim PlayerCount As Integer
    Dim WhoseTurn As Integer = 1
    Dim PScore() As Integer
    Dim bg As Image = Image.FromFile("BG.bmp")
    Dim PinT As Image = Image.FromFile("PinT.png")
    Dim PinS As Image = Image.FromFile("PinS.png")
    Dim ul As New Point(0, 0)
    Dim ur As New Point(500, 0)
    Dim ll As New Point(0, 500)
    Dim rect As Point() = {ul, ur, ll}

    Public Class CList
        Public first As circle
        Public Sub init()
            first = Nothing
        End Sub
        Public Sub insert(cc As circle)
            If first Is Nothing Then
                first = cc
            Else
                Dim cur As circle = first
                While cur.nxt IsNot Nothing
                    cur = cur.nxt
                End While
                cur.nxt = cc
            End If
        End Sub
        Public Sub delete(cc As circle)
            Dim e As circle = first
            If e Is cc Then
                first = e.nxt
            Else
                Dim e1 As circle = e.nxt
                While Not e1 Is cc
                    e = e1
                    e1 = e.nxt
                End While
                e.nxt = e1.nxt
            End If
        End Sub
        Public Sub deletestriker()
            If first.nxt Is Nothing Then
                first = Nothing
            Else
                Dim cur As circle = first
                Dim prev As circle
                While cur.target And cur IsNot Nothing
                    prev = cur
                    cur = cur.nxt
                End While
                If Not cur.target Then
                    prev.nxt = Nothing
                End If
            End If
        End Sub
    End Class

    Public Class circle
        Public x, y As Double
        Public nxt As circle
        Public degree, v As Double
        Public target As Boolean
        Public Sub New(xx As Double, yy As Double, t As Boolean)
            x = xx
            y = yy
            target = t
            degree = 0
            v = 0
            nxt = Nothing
        End Sub
        Public Sub updatepos()
            v -= friction
            If v < 0 Then
                v = 0
            End If
            x += v * Math.Cos(degree)
            y += v * Math.Sin(degree)
        End Sub
    End Class

    Sub drawPocket()
        Dim blackbrush As SolidBrush = New SolidBrush(Color.Black)
        g.FillRectangle(blackbrush, 0, 0, 50, 50)
        g.FillRectangle(blackbrush, 450, 0, 50, 50)
        g.FillRectangle(blackbrush, 0, 450, 50, 50)
        g.FillRectangle(blackbrush, 450, 450, 50, 50)
        PictureBox1.Image = btp
    End Sub

    Sub initBoard()
        g.Clear(Color.White)
        g.DrawImage(bg, rect)
        drawPocket()
    End Sub

    Sub initScore()
        Dim i As Integer
        For i = 0 To PlayerCount - 1
            ReDim Preserve PScore(i)
            PScore(i) = 0
        Next
    End Sub

    Public Function isCollide(c1 As circle, c2 As circle)
        If Math.Sqrt((c1.x - c2.x) * (c1.x - c2.x) + (c1.y - c2.y) * (c1.y - c2.y)) <= 50 Then
            Return True
        End If
        Return False
    End Function

    Function isHittingWall(c As circle)
        Dim side As String = ""
        If c.x < 25 Then
            side = "Left"
        ElseIf c.x > 474 Then
            side = "Right"
        End If
        If c.y < 25 Then
            side = "Top"
        ElseIf c.y > 474 Then
            side = "Bottom"
        End If
        Return side
    End Function

    Sub adjustCircleWall(c As circle, s As String)
        If s <> "" Then
            If s = "Left" Then
                c.x = 25
                c.degree = Math.PI - c.degree
            ElseIf s = "Top" Then
                c.y = 25
                c.degree = -c.degree
            ElseIf s = "Right" Then
                c.x = 474
                c.degree = Math.PI - c.degree
            Else
                c.y = 474
                c.degree = -c.degree
            End If
        End If
    End Sub

    Public Function EnterHole(cx As Double, cy As Double)
        If (cx <= 50 And cy <= 50) Or (cx >= 450 And cy <= 50) Or (cx <= 50 And cy >= 450) Or (cx >= 450 And cy >= 450) Then
            Return True
        End If
        Return False
    End Function

    Public Sub checkHoleCollision()
        Dim c As circle = listofdisks.first
        While c IsNot Nothing
            If EnterHole(c.x, c.y) Then
                listofdisks.delete(c)
                If c.target Then
                    updateScore(1)
                Else
                    updateScore(-1)
                    strikerexist = False
                End If
            End If
            c = c.nxt
        End While
    End Sub

    Public Sub checkBallCollission()
        Dim c1 As circle = listofdisks.first
        While Not IsNothing(c1)
            Dim c2 As circle = c1.nxt
            Dim e As Boolean = False
            While Not IsNothing(c2)
                If Not c2 Is c1 Then
                    If isCollide(c1, c2) Then
                        'if the audio is played, the collision will laggy
                        'My.Computer.Audio.Play("Tick.wav", AudioPlayMode.Background)
                        Dim dx, dy, distance As Double
                        dx = c1.x - c2.x
                        dy = c1.y - c2.y
                        distance = Math.Sqrt(dx * dx) + (dy * dy)

                        If c1.v < c2.v Then
                            c1.v = c2.v
                            c2.degree = (Math.Atan2(dy, dx))
                            c1.degree = Math.PI + c2.degree
                        Else
                            c2.v = c1.v
                            c1.degree = (Math.Atan2(dy, dx))
                            c2.degree = Math.PI + c1.degree
                        End If
                        splitcircles(c1, c2)
                    End If
                End If
                c2 = c2.nxt
            End While
            c1 = c1.nxt
        End While
    End Sub

    Function gameEnd() As Boolean 'true if the only disk remains on the board is striker
        Return Not listofdisks.first.target
    End Function

    Function isallstop() As Boolean 'check whether all the disks have stopped, true if all have stopped, false if otherwise
        Dim flag As Boolean = True
        If listofdisks.first IsNot Nothing Then
            Dim cur As circle = listofdisks.first
            While cur IsNot Nothing And flag
                If cur.v <> 0 Then
                    flag = False
                End If
                cur = cur.nxt
            End While
        End If
        Return flag
    End Function

    Private Sub PictureBox1_Mouse_Move(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        Label3.Text = e.X
        Label4.Text = e.Y
    End Sub

    Sub draw()
        If listofdisks.first IsNot Nothing Then
            Dim cur As circle = listofdisks.first
            While cur IsNot Nothing
                Dim ul As New Point(cur.x - 25, cur.y - 25)
                Dim ur As New Point(cur.x + 25, cur.y - 25)
                Dim ll As New Point(cur.x - 25, cur.y + 25)
                Dim pin As Point() = {ul, ur, ll}
                If cur.target Then
                    g.DrawImage(PinT, pin)
                Else
                    g.DrawImage(PinS, pin)
                End If
                cur = cur.nxt
            End While
        End If
        PictureBox1.Image = btp
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        initBoard()
        temp = New circle(PictureBox1.Width / 2, PictureBox1.Height / 2, True)
        listofdisks.insert(temp)
        temp = Nothing
        draw()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If addtarget Then
            addtarget = False
            Button1.Text = "Add Target"
        Else
            addtarget = True
            Button1.Text = "Done"
        End If
    End Sub

    Function badPlacement(c As circle) As Boolean 'true if the placement of the disk is colliding with other disk or hole or wall
        Dim flag As Boolean = False
        If Not EnterHole(c.x, c.y) Then
            If isHittingWall(c) = "" Then
                Dim cur As circle = listofdisks.first
                While cur IsNot Nothing AndAlso cur IsNot c AndAlso Not flag AndAlso (cur.target Or c.target)
                    If isCollide(c, cur) Then
                        flag = True
                        MsgBox("Disk cannot be placed near other disk.")
                    End If
                    cur = cur.nxt
                End While
            Else 'isHittingWall <> ""
                flag = True
                MsgBox("Disk cannot be placed near wall.")
            End If
        Else 'EnterHole(c.x,c.y)
            flag = True
            MsgBox("Pin cannot be placed over the hole.")
        End If
        If Not addtarget Then
            If flag And strikerexist Then
                listofdisks.deletestriker()
                strikerexist = False
                initBoard()
                draw()
            End If
        End If
        Return flag
    End Function

    Sub updateScore(score As Integer)
        Select Case WhoseTurn
            Case 1
                PScore(0) += score
                Score1.Text = "Player 1 : " + PScore(0).ToString
            Case 2
                PScore(1) += score
                Score2.Text = "Player 2 : " + PScore(1).ToString
            Case 3
                PScore(2) += score
                Score3.Text = "Player 3 : " + PScore(2).ToString
            Case Else
                PScore(3) += score
                Score4.Text = "Player 4 : " + PScore(3).ToString
        End Select
    End Sub

    Function decideWinner() As String
        Dim winners As String = ""
        If PlayerCount = 1 Then
            winners = "Player 1"
        Else
            Dim i As Integer
            winners = "Player 1"
            Dim maxValue As Integer = PScore(0)
            For i = 1 To PlayerCount - 1
                If maxValue < PScore(i) Then
                    maxValue = PScore(i)
                    'winnerIndex = i + 1
                    winners = "Player " + (i + 1).ToString
                ElseIf maxValue = PScore(i) Then
                    winners += ", Player " + (i + 1).ToString
                End If
            Next
        End If
        Return winners
    End Function

    Sub splitcircles(c1 As circle, c2 As circle)
        Dim d As Double
        Dim dx, dy As Double
        dx = c2.x - c1.x
        dy = c2.y - c1.y
        d = Math.Sqrt((dx * dx) + (dy * dy))
        Dim deg As Double = Math.Atan2(dy, dx)
        Dim seperation As Integer = radius + radius - d
        If c2.v = 0 Then
            c1.x -= Math.Cos(deg) * seperation
            c1.y -= Math.Sin(deg) * seperation
        ElseIf c1.v = 0 Then
            c2.x += Math.Cos(deg) * seperation
            c2.y += Math.Sin(deg) * seperation
        Else
            c1.x -= Math.Cos(deg) * seperation / 2
            c1.y -= Math.Sin(deg) * seperation / 2
            c2.x += Math.Cos(deg) * seperation / 2
            c2.y += Math.Sin(deg) * seperation / 2
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        initBoard()
        If listofdisks.first IsNot Nothing Then
            Dim cur As circle = listofdisks.first
            Dim side As String
            While cur IsNot Nothing
                cur.updatepos()
                'check whether circle hit wall and also decide wich side it hit
                side = isHittingWall(cur)
                'adjust the x or y based on which side gets hit
                adjustCircleWall(cur, side)
                cur = cur.nxt
            End While
            checkHoleCollision()

            checkBallCollission()
            'splitcircles()
        End If

        If isallstop() Then
            Timer1.Enabled = False
            If strikerexist Then
                listofdisks.deletestriker()
                strikerexist = False
            End If
            If listofdisks.first IsNot Nothing Then
                If WhoseTurn < PlayerCount Then
                    WhoseTurn += 1
                Else
                    WhoseTurn = 1
                End If
                Turn.Text = "Player " + WhoseTurn.ToString + " Turn"
            Else

                MsgBox("Game End")
                Dim winner As String = decideWinner()
                MsgBox("Congratulation: " + winner + " for winning the game!")
                PictureBox1.Visible = False
                GroupBox1.Visible = False
                GroupBox2.Visible = False
                GroupBox3.Visible = False
                GroupBox4.Visible = False
                Turn.Visible = False
                Label8.Visible = True
            End If
            Button1.Enabled = True
            launch = False
            Button2.Enabled = True
            Button2.Text = "Launch"
            PictureBox1.Enabled = True
            TextBox1.Enabled = True
        End If
        draw()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If launch Then
            launch = False
            Button1.Enabled = True
            Button2.Text = "Launch"
            TextBox1.Enabled = True
        Else
            If strikerexist Then
                launch = True
                Button1.Enabled = False
                Button2.Text = "Cancel"
                TextBox1.Enabled = False
            Else
                MsgBox("Place the striker pin.")
            End If
        End If
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As MouseEventArgs) Handles PictureBox1.Click
        If launch Then
            If IsNumeric(TextBox1.Text) Then
                Dim cur As circle = listofdisks.first
                Dim prev As circle
                While cur.target
                    prev = cur
                    cur = cur.nxt
                End While
                If CDbl(TextBox1.Text) > 15 Then
                    MsgBox("The maximum speed is 15")
                Else
                    cur.v = CDbl(TextBox1.Text)
                End If
                cur.degree = Math.Atan2(e.Y - cur.y, e.X - cur.x)
                Button2.Enabled = False
                Timer1.Enabled = True
                PictureBox1.Enabled = False
            Else
                MsgBox("The input must be numeric")
                launch = False
                Button1.Enabled = True
                Button2.Text = "Launch"
                TextBox1.Enabled = True
                TextBox1.Text = "5"
            End If
        Else
            temp = New circle(e.X, e.Y, addtarget)
            If Not badPlacement(temp) Then
                If addtarget Then
                    If strikerexist Then
                        initBoard()
                        listofdisks.deletestriker()
                        strikerexist = False
                    End If
                Else
                    If Not strikerexist Then
                        strikerexist = True
                    Else
                        initBoard()
                        listofdisks.deletestriker()
                    End If
                End If
                listofdisks.insert(temp)
                draw()
            End If
        End If
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If TextBox2.Text <> "" And IsNumeric(TextBox2.Text) Then
            PlayerCount = CInt(TextBox2.Text)
            If PlayerCount > 0 And PlayerCount < 5 Then
                'if it is played, the collision will laggy
                My.Computer.Audio.Play("play.wav", AudioPlayMode.Background)
                PictureBox1.Visible = True
                GroupBox1.Visible = True
                GroupBox2.Visible = True
                GroupBox3.Visible = False
                GroupBox4.Visible = True
                Turn.Visible = True
                Select Case PlayerCount
                    Case 1
                        Score1.Visible = True
                    Case 2
                        Score1.Visible = True
                        Score2.Visible = True
                    Case 3
                        Score1.Visible = True
                        Score2.Visible = True
                        Score3.Visible = True
                    Case Else
                        Score1.Visible = True
                        Score2.Visible = True
                        Score3.Visible = True
                        Score4.Visible = True
                End Select
                initScore()
            Else 'PlayerCount <= 0
                MsgBox("Please enter the correct player(s) number.")
            End If
        ElseIf TextBox2.Text = "" Then
            MsgBox("Please enter player(s) number.")
        Else
            MsgBox("The input must be numeric")
        End If
    End Sub

End Class
