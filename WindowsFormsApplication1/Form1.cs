using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Private
        /// <param name="sourceBitmap">이미지 파일</param>
        /// <param name="left_up">자를 이미지 구역 포인트</param>
        /// <param name="ref_rect">자를 이미지 사각형</param>
        /// <param name="ptCursor">사각형내의 마우스 위치</param>
        private Bitmap sourceBitmap = null; //메인 이미지
        private Image cropImagemap = null; //자른 이미지
        private Image backImage = null;//회색 메인 이미지

        private Rectangle ref_rect;
        private Pen blackPen = new Pen(Color.Black, 3);
        private Point ptCursor;
        private string moveFlag; // 마우스 이동 타입 지정


        private int left_up; // 사각형 x   ref_rect.X
        private int left_dn; // 사격형 y   ref_rect.Y
        private int right_up; //사각형 길이  ref_rect.Width
        private int right_dn; //사각형 높이  ref_rect.Height

        private int d_left_up; //마우스 클릭 시, 사각형 왼쪽 포인트 지점
        private int d_left_dn; //마우스 클릭 시, 사각형 위 포인트 지점
        private int d_right_up; //마우스 클릭 시, 사각형 오른쪽 포인트 지점
        private int d_right_dn; //마우스 클릭 시, 사각형 아래 포인트 지점
        //////////////////////////////////////////////////////////////////////////////////////////////////// Constructor
        ////////////////////////////////////////////////////////////////////////////////////////// Public
        #region 생성자 - 메인 Form1()
        public Form1()
        {
            InitializeComponent();
        }
        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Private
        #region 버튼 Method
        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                //파일 읽어오는 다이어로그 창
                using (OpenFileDialog ofd = new OpenFileDialog() { InitialDirectory = "..\\", Filter = "JPG File(*.jpg) | *.jpg" })
                {
                    //ofd의 시작위치 결정 eg.) "C:\\"
                    //ofd.Filter = "모든 파일 (*.*) | *.*";

                    //ofd.InitialDirectory = "..\\";
                    //ofd.Filter = "JPG File(*.jpg) | *.jpg";

                    ofd.ShowDialog();
                    string pathRead = ofd.FileName;


                    //Load시 네이티브 프레임 호출 스택관련해서 행이 걸림. 
                    if (pathRead == "")
                    {
                        Console.Write("File name is empty");
                    }
                    else
                    {
                        sourceBitmap = null; //메인 이미지
                        cropImagemap = null; //자른 이미지
                        backImage = null;//회색 메인 이미지
                        pictureBox1.Image = null;

                        this.sourceBitmap = LoadBitmapUnlocked(pathRead); 
                        pictureBox1.BackgroundImage = this.sourceBitmap;
                        pictureBox1.BackgroundImageLayout = ImageLayout.None;

                        left_up = pictureBox1.Location.X;
                        left_dn = pictureBox1.Location.X;

                        if (this.sourceBitmap.Width < this.pictureBox1.Width)
                        {
                            right_up = this.sourceBitmap.Width;
                        }
                        else
                        {
                            right_up = this.pictureBox1.Width;
                        }

                        if (this.sourceBitmap.Height < this.pictureBox1.Height)
                        {
                            right_dn = this.sourceBitmap.Height;
                        }
                        else
                        {
                            right_dn = this.pictureBox1.Height;
                        }
                        
                        d_left_up = left_up;
                        d_left_dn = left_dn;
                        d_right_up = right_up;
                        d_right_dn = right_dn;
                    }

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// checkBox1 : 자르기 checkbox 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Console.WriteLine("Check");

                if (this.backImage != null)
                {
                    this.pictureBox1.Image = null; 
                    this.pictureBox1.BackgroundImage = this.backImage;
                }
            }
            else
            {
                Console.WriteLine("No Check");

                if (this.sourceBitmap != null)
                {
                    this.cropImagemap = cropImage1(this.sourceBitmap, ref_rect);
                    this.pictureBox1.Image = this.cropImagemap; 
                    this.pictureBox1.BackgroundImage = null;

                    this.backImage = ChangeOpacity(this.sourceBitmap);
                }
            }
        }

        #endregion
        /// <summary>
        /// 잠금없이 비트맵 로드하기
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <returns>비트맵</returns>
        private Bitmap LoadBitmapUnlocked(string filePath)
        {
            using (Bitmap bitmap = new Bitmap(filePath))
            {
                return new Bitmap(bitmap);
            }

        }
        //not used
        protected override void OnPaint(PaintEventArgs e)
        {
            // 상위 클랫를 실행
            base.OnPaint(e);
        }
        // 이미지 자르기
        private static Image cropImage1(Image img, Rectangle cropArea)
        {
            try
            {
                Bitmap bmp = new Bitmap(cropArea.Width, cropArea.Height);
                using (Graphics gph = Graphics.FromImage(bmp))
                {
                    gph.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), cropArea, GraphicsUnit.Pixel);
                }
                return bmp;

            }
            catch
            {
                Console.WriteLine("CropImage Error");
                return null;
            }
        }
        //이미지 회색으로 바꾸기
        public Bitmap ChangeOpacity(Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                        {
                            new float[]{.3f, .3f, .3f, 0, 0},
                            new float[]{.39f, .39f, .39f, 0, 0},
                            new float[]{.11f, .11f, .1f, 0, 0},
                            new float[]{0, 0, 0, 1, 0},
                            new float[]{0, 0, 0, 0, 1}
                        });

            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            using (Graphics gph = Graphics.FromImage(bmp))
            {
                gph.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            }

            return bmp;
        }

        #region 캔버스 픽처 박스 페인트시 처리하기 - PictureBox1_Paint(sender, e)
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (checkBox1.Checked)
            {
                ref_rect = new Rectangle(left_up, left_dn, right_up, right_dn);

                if (this.cropImagemap != null)
                {
                    e.Graphics.DrawImage(this.cropImagemap, ref_rect);
                }

                // 자르기 구역 사각형
                blackPen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(blackPen, ref_rect);


                // Display heavier corners
                Pen BluePen = new Pen(Color.Blue, 10);
                //BluePen.Alignment = PenAlignment.Inset; //<-- this

                int x = d_right_up - d_left_up - 100;
                //BluePen.DashPattern = new float[] { 5.0F, x };
                //왼쪽 위 코너 라인
                e.Graphics.DrawLine(BluePen, d_left_up, d_left_dn, d_left_up + 10, d_left_dn);
                e.Graphics.DrawLine(BluePen, d_left_up, d_left_dn - 5, d_left_up, d_left_dn + 10);
                //왼쪽 아래 코너 라인
                e.Graphics.DrawLine(BluePen, d_left_up, d_right_dn, d_left_up + 10, d_right_dn);
                e.Graphics.DrawLine(BluePen, d_left_up, d_right_dn + 5, d_left_up, d_right_dn -10);
                //오른쪽 위 코너 라인
                e.Graphics.DrawLine(BluePen, d_right_up, d_left_dn, d_right_up - 10, d_left_dn);
                e.Graphics.DrawLine(BluePen, d_right_up, d_left_dn - 5, d_right_up, d_left_dn + 10);
                //오른쪽 아래 코너 라인
                e.Graphics.DrawLine(BluePen, d_right_up, d_right_dn, d_right_up - 10, d_right_dn);
                e.Graphics.DrawLine(BluePen, d_right_up, d_right_dn +5, d_right_up, d_right_dn - 10);
            }

            this.pictureBox1.Invalidate();
        }
        #endregion
        #region 캔버스 픽처 박스 마우스 DOWN 처리하기 - PictureBox1_MouseDown(sender, e)
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ptCursor = e.Location;

                    Console.WriteLine(ptCursor);
                    Console.WriteLine("left_up : " + left_up + " left_dn : " + left_dn + " right_up : " + right_up + " right_dn : " + right_dn);
                    Console.WriteLine("#### left_up : " + d_left_up + " left_dn : " + d_left_dn + " right_up : " + d_right_up + " right_dn : " + d_right_dn);

                    
                    //세로선 외곽선
                    if ((d_left_up - 5 < e.X && e.X < d_left_up + 5))//왼쪽 
                    {
                        if ((d_left_dn - 5 < e.Y && e.Y < d_left_dn + 5))
                        {
                            moveFlag = "leftUp";
                        }
                        else if ((d_right_dn - 5 < e.Y && e.Y < d_right_dn + 5))
                        {
                            moveFlag = "leftDn";
                        }
                        else
                        {
                            //Console.WriteLine("left_up : " + e.X);
                            moveFlag = "left";
                            Cursor.Current = Cursors.IBeam;
                        }
                    } 
                    else if ((d_right_up - 5 < e.X && e.X < d_right_up + 5))//오른쪽
                    {
                        if ((d_left_dn - 5 < e.Y && e.Y < d_left_dn + 5))
                        {
                            moveFlag = "rightUp";
                        }
                        else if ((d_right_dn - 5 < e.Y && e.Y < d_right_dn + 5))
                        {
                            moveFlag = "rightDn";
                        }
                        else
                        {
                            //Console.WriteLine("right_up : " + e.X);
                            moveFlag = "right";
                            Cursor.Current = Cursors.IBeam;
                        }
                    }
                    //가로선 외곽선
                    else if ((d_left_dn - 5 < e.Y && e.Y < d_left_dn + 5)) //위
                    {
                        //Console.WriteLine("left_dn : " + e.Y);
                        moveFlag = "top";
                        Cursor.Current = Cursors.Hand;
                    }
                    else if ((d_right_dn - 5 < e.Y && e.Y < d_right_dn + 5)) //아래
                    {
                        //Console.WriteLine("right_dn : " + e.Y);
                        moveFlag = "bottom";
                        Cursor.Current = Cursors.Hand;
                    }
                    //사각형 내부
                    else if (e.X > d_left_up + 5 && e.X < d_right_up - 5 && e.Y > d_left_dn + 5 && e.Y < d_right_dn - 5)
                    {
                        moveFlag = "middle";
                        Cursor.Current = Cursors.Cross;
                    }
                    //else
                    //{
                    //    Cursor.Current = Cursors.Default;
                    //    moveFlag = "None";
                    //}
                }
            }

            
        }
        #endregion
        #region 캔버스 픽처 박스 마우스 UP 처리하기 - PictureBox1_MouseUp(sender, e)
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            moveFlag = "None";

            this.pictureBox1.Invalidate();
            this.pictureBox1.Refresh();
        }
        #endregion
        #region 캔버스 픽처 박스 마우스 이동시 처리하기 - PictureBox1_MouseMove(sender, e)
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (e.Button == MouseButtons.Left)
                {
                    var pos = e.Location;
                    this.cropImagemap = cropImage1(this.sourceBitmap, ref_rect);

                    int distanceX = d_right_up - pos.X;
                    int distanceXr = pos.X - d_left_up;
                    int distanceY = d_right_dn - pos.Y;
                    int distanceYr = pos.Y - d_left_dn;

                    switch (moveFlag)
                    {
                        case "left": //왼쪽 
                            if (0 <= pos.X - d_right_up) //마우스 X좌표가 오른쪽 선에 닿기 전까지
                            {
                                //빠르게 움직일 때 기준점 흔들리는 현상 방지
                                left_up = d_right_up;
                                d_left_up = left_up;

                                moveFlag = "right";
                            }
                            else
                            {
                                left_up = pos.X;
                                d_left_up = left_up;
                                right_up = distanceX;
                            }
                            break;

                        case "right": //오른쪽
                            Console.WriteLine("right_up"); // done

                            if (d_left_up - pos.X >= 0)
                            {
                                d_right_up = d_left_up;
                                moveFlag = "left";
                            }
                            else
                            {
                                right_up = distanceXr;
                                d_right_up = pos.X;
                            }
                            break;

                        case "top": //위
                            if (0 <= pos.Y - d_right_dn)
                            {
                                left_dn = d_right_dn;
                                d_left_dn = left_dn;

                                moveFlag = "bottom";
                            }
                            else
                            {
                                left_dn = pos.Y;
                                d_left_dn = left_dn;
                                right_dn = distanceY;
                            }

                            break;

                        case "bottom": //아래
                            if (d_left_dn - pos.Y >= 0)
                            {
                                d_right_dn = d_left_dn;
                                moveFlag = "top";
                            }
                            else
                            {
                                right_dn = distanceYr;
                                d_right_dn = pos.Y;
                            }
                            break;

                        case "middle":
                            break;

                        case "leftUp":
                            if(0 <= pos.X - d_right_up){
                                left_up = d_right_up;
                                d_left_up = left_up;

                                moveFlag = "rightUp";
                            }
                            else if (0 <= pos.Y - d_right_dn)
                            {
                                left_dn = d_right_dn;
                                d_left_dn = left_dn;

                                moveFlag = "leftDn";
                            }
                            else{
                                left_up = pos.X;
                                left_dn = pos.Y;
                            
                                d_left_up = left_up;
                                d_left_dn = left_dn;

                                right_up = distanceX;
                                right_dn = distanceY;
                            }
                            break;

                        case "leftDn":
                            if (d_left_dn - pos.Y >= 0)
                            {
                                d_right_dn = left_dn;
                                d_left_dn = d_right_dn;

                                moveFlag = "leftUp";
                            }
                            else if (0 <= pos.X - d_right_up)
                            {
                                d_left_up = d_right_up;
                                left_up = d_left_up;
                                moveFlag = "rightDn";
                            }
                            else{
                                left_up = pos.X;
                                d_left_up = left_up;
                                right_up = distanceX;

                                right_dn = distanceYr;
                                d_right_dn = pos.Y;
                            }
                            break;

                        case "rightUp":
                            if(d_left_up - pos.X >= 0){
                                d_right_up = d_left_up;
                                moveFlag = "leftUp";
                            }
                            else if (0 <= pos.Y - d_right_dn)
                            {
                                d_left_dn = d_right_dn;
                                left_dn = d_left_dn;
                                moveFlag = "rightDn";
                            }
                            else
                            {
                                left_dn = pos.Y;
                                d_left_dn = left_dn;
                                right_dn = distanceY;

                                right_up = distanceXr;
                                d_right_up = pos.X;
                            }
                            break;

                        case "rightDn":
                            if (d_left_dn - pos.Y >= 0)
                            {
                                d_right_dn = d_left_dn;
                                d_left_dn = d_right_dn;

                                moveFlag = "rightUp";

                            }
                            else if (d_left_up - pos.X >= 0)
                            {
                                d_right_up = d_left_up;
                                moveFlag = "leftDn";
                            }
                            else
                            {
                                right_up = distanceXr;
                                d_right_up = pos.X;

                                right_dn = distanceYr;
                                d_right_dn = pos.Y;
                            }
                            break;
                    }
                    Console.WriteLine("moveFlag : " + moveFlag);
                }
            }
        }
        #endregion
    }
}
