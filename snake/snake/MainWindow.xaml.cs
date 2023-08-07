using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace snake
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PANNELWID = 640;  //画板宽度
        const int PANNELHEI = 480;  //画板高度
        const int CELLSIZE = 20;//小格子大小
        const int SNAKEHEAD = 0;//蛇头位置
        const int CELLWIDTH = PANNELWID/ CELLSIZE;//游戏区横格数
        const int CELLHEIGHT = PANNELHEI / CELLSIZE;//游戏区纵格数

        //蛇身前进方向
        enum Direction{
            Up, Down, Left, Right
        }
        Direction direction = Direction.Up;
        

        //游戏状态
        enum GameState
        {
            NONE,
            GAMEING,
            PAUSE,
            STOP
        }
        
        GameState currentGameState=GameState.NONE;

        List<SnakeNode> snakeNodeList= new List<SnakeNode>();//蛇身List
        Fruit fruit;
        Random rnd = new Random((int)DateTime.Now.Ticks);    //随机数
        System.Windows.Threading.DispatcherTimer timer=new System.Windows.Threading.DispatcherTimer();//计时器
        public MainWindow()
        {
            InitializeComponent();
            DrawGrid();
            timer.Interval = new TimeSpan(0,0,0,0,160);
            timer.Tick += Timer_Tick;
        }
        /// <summary>
        /// //计时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckCollision();
            GenNewSnakeNode();

            if (IsGameOver())
            {
                currentGameState = GameState.STOP;
                timer.Stop();
                MessageBox.Show("游戏结束！","警告",MessageBoxButton.OK);
                return;
            }

        }

        //绘制游戏区暗格线
        private void DrawGrid()
        {
            Path gridPath = new Path();
            gridPath.Stroke=new SolidColorBrush(Color.FromArgb(255,50,50,50));
            gridPath.StrokeThickness = 1;
            StringBuilder sb = new StringBuilder();

            for(int x=0; x < PANNELWID; x += CELLSIZE)
            {
                sb.Append($"M{x},0 L{x},480");
            }
            for(int y=0;y< PANNELHEI;y+= CELLSIZE)
            {
                sb.Append($"M0,{y}L640,{y}");
            }

            gridPath.Data=Geometry.Parse( sb.ToString() );
            
            myCanvas.Children.Add( gridPath );
        }

        //随机水果位置
        private Point SetFriutToRandomPostion()
        {
            //和蛇身不能重叠，重叠则重新生成
            bool flag = true;
            Point pos=new Point();
            while (flag)
            {
                flag = false;
                pos = new Point(rnd.Next(0, CELLWIDTH), rnd.Next(0, CELLHEIGHT));
                for(int i=0;i< snakeNodeList.Count; i++)
                {
                    if (pos.X == snakeNodeList[i]._pos.X && pos.Y == snakeNodeList[i]._pos.Y)
                    {
                        flag=true; 
                        break;
                    }
                }
            }
            return pos;
        }

        //生成新的蛇身
        private void GenNewSnakeNode()
        {
            /*原理就是，游戏运行之后，通过计时器事件不断生成新的单节蛇身类
             SnakeNode，添加到List中0的位置，原来的蛇头变成来了第二节。该节新
             蛇头的坐标通过蛇头前景方向Directi进行判断，即如果原来蛇头往左运行
             则新的蛇头在原蛇头位置的左一格生成，其它方向以此类推。
             最后将该节点添加到SnakeNode列表中，并将相应图形添加进游戏区
             */

            SnakeNode snakeNode = null;
            switch (direction)
            {
                case Direction.Up:
                    snakeNode = new SnakeNode(new Point(snakeNodeList[SNAKEHEAD]._pos.X,snakeNodeList[SNAKEHEAD]._pos.Y - 1)) ;
                    break;
                case Direction.Down:
                    snakeNode = new SnakeNode(new Point(snakeNodeList[SNAKEHEAD]._pos.X,snakeNodeList[SNAKEHEAD]._pos.Y + 1));
                    break;
                case Direction.Left:
                    snakeNode = new SnakeNode(new Point(snakeNodeList[SNAKEHEAD]._pos.X-1,snakeNodeList[SNAKEHEAD]._pos.Y));
                    break;
                case Direction.Right:
                    snakeNode = new SnakeNode(new Point(snakeNodeList[SNAKEHEAD]._pos.X+1,snakeNodeList[SNAKEHEAD]._pos.Y));
                    break;
            }
            if(snakeNode != null)
            {
                snakeNodeList.Insert(0, snakeNode);
                myCanvas.Children.Add(snakeNodeList[0]._rect);
            }
        }

        //碰撞监测（蛇头与水果）
        private void CheckCollision()
        {
            /*
             因为蛇头会先触碰水果，所以只需要判断蛇头坐标与说过坐标是否相同即可
             如果撞到水果，则将水果随机生成新的位置
             如果没有碰到
             则删除蛇尾一节（因为之后会通过计时器生成新的蛇头）
             这样保存蛇身长度感觉位置像前进了一格
             */
            if (snakeNodeList[SNAKEHEAD]._pos.X == fruit._pos.X && snakeNodeList[SNAKEHEAD]._pos.Y==fruit._pos.Y)
            {
                fruit.SetPosition(SetFriutToRandomPostion());
            }
            else
            {
                if (myCanvas.Children.Contains(snakeNodeList[snakeNodeList.Count - 1]._rect))
                {
                    myCanvas.Children.Remove(snakeNodeList[snakeNodeList.Count - 1]._rect);
                }
                snakeNodeList.RemoveAt(snakeNodeList.Count - 1);
            }
        }

        //删除所有蛇身节点，重新开始游戏用到
        private void RemoveSnakeNodeAll()
        {
            for(int i = 0; i < snakeNodeList.Count; i++)
            {
                if (myCanvas.Children.Contains(snakeNodeList[i]._rect))
                {
                    myCanvas.Children.Remove(snakeNodeList[i]._rect);
                }
            }
        }
        //删除水果
        private void RemoveFruit()
        {
            if (fruit == null)
            {
                return;
            }
            if (myCanvas.Children.Contains(fruit._ellipse))
            {
                myCanvas.Children.Remove(fruit._ellipse);
            }
        }
        //游戏开始
        private void StartGame()
        {
            RemoveSnakeNodeAll();
            RemoveFruit();

            int startX = rnd.Next(5, CELLWIDTH - 6);
            int startY=rnd.Next(5, CELLHEIGHT - 6);
            direction = Direction.Right;

            fruit = new Fruit(SetFriutToRandomPostion(), myCanvas);

            snakeNodeList=new List<SnakeNode>();
            snakeNodeList.Add(new SnakeNode(new Point(startX, startY)));
            GenNewSnakeNode();
            GenNewSnakeNode();
        }

        //判断游戏是否结束
        private bool IsGameOver()
        {
            /*
             很简单，就是判断蛇头是否碰到蛇身或者游戏四边形范围则返回TRUE
              
             */
            bool isGameOver = false;
            if (snakeNodeList[SNAKEHEAD]._pos.X == -1 ||snakeNodeList[SNAKEHEAD]._pos.X==CELLWIDTH
                ||snakeNodeList[SNAKEHEAD]._pos.Y == -1 || snakeNodeList[SNAKEHEAD]._pos.Y== CELLHEIGHT)
            {
                isGameOver = true;
            }

            foreach (var node in snakeNodeList)
            {
                if (node == snakeNodeList[SNAKEHEAD])
                {
                    continue;
                }
                if (node._pos.X == snakeNodeList[SNAKEHEAD]._pos.X&&
                   node._pos.Y == snakeNodeList[SNAKEHEAD]._pos.Y)
                {
                    isGameOver = true;
                }

            }
            return isGameOver;
        }


        //菜单开始事件
        private void MenuFile_NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
            timer.Start();
            currentGameState = GameState.GAMEING;
            MenuControl_Pause.Header = "暂停";
        }

        //键盘控制事件
        private void myCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            switch(e.Key)
            {
                case Key.Left:
                    if(direction!=Direction.Right)
                    {
                        direction = Direction.Left;
                    }
                    break;
                case Key.Right:
                    if (direction != Direction.Left)
                    {
                        direction = Direction.Right;
                    }
                    break;
                case Key.Up:
                    if(direction != Direction.Down)
                    {
                        direction = Direction.Up;
                    }
                    break;
                case Key.Down:
                    if(direction != Direction.Up)
                    {
                        direction = Direction.Down;
                    }
                    break;
                case Key.Escape:
                    Application.Current.Shutdown();
                    break;
                case Key.Space:
                    if(currentGameState == GameState.NONE)
                    {
                        return;
                    }
                    if(currentGameState == GameState.PAUSE)
                    {
                        currentGameState = GameState.GAMEING;
                        timer.Start();
                        MenuControl_Pause.Header = "暂停";
                    }
                    else if(currentGameState == GameState.GAMEING)
                    {
                        currentGameState = GameState.PAUSE;
                        timer.Stop();
                        MenuControl_Pause.Header = "继续";
                    }
                    break;
            }
        }


    }
    //添加水果类
    public class Fruit
    {
        public Point _pos {  get; set; }
        public Ellipse _ellipse { get; set; }

        public Canvas _canvas { get; set; }

        public Fruit(Point point,Canvas canvas)
        {
            _pos = point;
            _canvas = canvas;

            _ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };

            _ellipse.SetValue(Canvas.LeftProperty, _pos.X*20);
            _ellipse.SetValue(Canvas.TopProperty,_pos.Y*20);
            _canvas.Children.Add( _ellipse );
        }


        public void SetPosition(Point position)
        {
            _pos = position;
            _ellipse.SetValue(Canvas.LeftProperty,_pos.X*20);
            _ellipse.SetValue(Canvas.TopProperty, _pos.Y * 20);
        }
    }

    //添加蛇类
    public class SnakeNode
    {
        public Point _pos { get; set; }
        public Rectangle _rect { get; set; }
        public SnakeNode(Point point) 
        {
            _pos= point;
            _rect = new Rectangle
            {
                Width = 20,
                Height = 20,
                Stroke = new SolidColorBrush(Colors.DodgerBlue),
                StrokeThickness = 3,
                Fill = Brushes.SkyBlue
            };

            _rect.SetValue(Canvas.LeftProperty, _pos.X * 20);
            _rect.SetValue(Canvas.TopProperty,_pos.Y * 20);
        }
    }
}
