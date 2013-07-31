/* Copyright (C) 2007 Jeff Morton jeffrey.raymond.morton@gmail.com
 
   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.
 
   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SoundViewer
{
    public partial class Form1 : Form
    {
        private WaveInRecorder _recorder;
        private byte[] _recorderBuffer;
        private WaveOutPlayer _player;
        private byte[] _playerBuffer;
        private FifoStream _stream;
        private WaveFormat _waveFormat;
        private AudioFrame _audioFrame;
        private int _audioSamplesPerSecond = 44100;
        private int _audioFrameSize = 16384;
        private byte _audioBitsPerSample = 16;
        private byte _audioChannels = 2;
        private bool _isPlayer = false;
        private bool _isTest = false;
        private int maxSample = 9;
        private double dist1 = 1;

        public Image map = new Bitmap(600, 600);
        private double i1x, i1y, i2x, i2y;
        double p2x, p2y;
        double distFrom0to2;
        bool found = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (WaveNative.waveInGetNumDevs() == 0)
            {
                textBox1.AppendText(DateTime.Now.ToString() + " : There are no audio devices available\r\n");
            }
            else
            {
                if (_isPlayer == true)
                    _stream = new FifoStream();
                _audioFrame = new AudioFrame(_isTest);
                Start();
            }
        }

        private void Start()
        {
            Stop();
            try
            {
                _waveFormat = new WaveFormat(_audioFrameSize, _audioBitsPerSample, _audioChannels);
                _recorder = new WaveInRecorder(0, _waveFormat, _audioFrameSize * 2, 3, new BufferDoneEventHandler(DataArrived));
                if (_isPlayer == true)
                    _player = new WaveOutPlayer(-1, _waveFormat, _audioFrameSize * 2, 3, new BufferFillEventHandler(Filler));
                textBox1.AppendText(DateTime.Now.ToString() + " : Audio device initialized\r\n");
                textBox1.AppendText(DateTime.Now.ToString() + " : Audio device polling started\r\n");
                textBox1.AppendText(DateTime.Now + " : Samples per second = " + _audioSamplesPerSecond.ToString() + "\r\n");
                textBox1.AppendText(DateTime.Now + " : Frame size = " + _audioFrameSize.ToString() + "\r\n");
                textBox1.AppendText(DateTime.Now + " : Bits per sample = " + _audioBitsPerSample.ToString() + "\r\n");
                textBox1.AppendText(DateTime.Now + " : Channels = " + _audioChannels.ToString() + "\r\n");
            }
            catch (Exception ex)
            {
                textBox1.AppendText(DateTime.Now + " : Audio exception\r\n" + ex.ToString() + "\r\n");
            }
        }

        private void Stop()
        {
            if (_recorder != null)
                try
                {
                    _recorder.Dispose();
                }
                finally
                {
                    _recorder = null;
                }
            if (_isPlayer == true)
            {
                if (_player != null)
                    try
                    {
                        _player.Dispose();
                    }
                    finally
                    {
                        _player = null;
                    }
                _stream.Flush(); // clear all pending data
            }
        }

        private void Filler(IntPtr data, int size)
        {
            if (_isPlayer == true)
            {
                if (_playerBuffer == null || _playerBuffer.Length < size)
                    _playerBuffer = new byte[size];
                if (_stream.Length >= size)
                    _stream.Read(_playerBuffer, 0, size);
                else
                    for (int i = 0; i < _playerBuffer.Length; i++)
                        _playerBuffer[i] = 0;
                System.Runtime.InteropServices.Marshal.Copy(_playerBuffer, 0, data, size);
            }
        }

        private void DataArrived(IntPtr data, int size)
        {
            if (_recorderBuffer == null || _recorderBuffer.Length < size)
                _recorderBuffer = new byte[size];
            if (_recorderBuffer != null)
            {
                System.Runtime.InteropServices.Marshal.Copy(data, _recorderBuffer, 0, size);
                if (_isPlayer == true)
                    _stream.Write(_recorderBuffer, 0, _recorderBuffer.Length);
                _audioFrame.Process(ref _recorderBuffer);
                _audioFrame.RenderTimeDomain(ref pictureBox1);
                _audioFrame.RenderFrequencyDomain(ref pictureBox2);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _audioFrame.number++;
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

int scale = 1;
            Font f = new Font(FontFamily.GenericSansSerif, 8);
            Brush b = new SolidBrush(Color.Blue);
            Graphics mapg = Graphics.FromImage(map);
            mapg.DrawString("drawing", f, b, 0, 0);

            mapg.DrawEllipse(new Pen(b), (float)(_audioFrame.mic1x * scale) - 5, (float)(_audioFrame.mic1y * scale) - 5, 10, 10);
            mapg.DrawEllipse(new Pen(b), (float)(_audioFrame.mic2x * scale) - 5, (float)(_audioFrame.mic2y * scale) - 5, 10, 10);
            mapg.DrawEllipse(new Pen(b), (float)(_audioFrame.mic3x * scale) - 5, (float)(_audioFrame.mic3y * scale) - 5, 10, 10);

            mapg.DrawLine(new Pen(b), (float)(_audioFrame.mic1x * scale), (float)(_audioFrame.mic1y * scale), (float)(_audioFrame.mic2x * scale), (float)(_audioFrame.mic2y * scale));

            mapg.DrawLine(new Pen(b), (float)(_audioFrame.mic2x * scale), (float)(_audioFrame.mic2y * scale), (float)(_audioFrame.mic3x * scale), (float)(_audioFrame.mic3y * scale));

            mapg.DrawLine(new Pen(b), (float)(_audioFrame.mic1x * scale), (float)(_audioFrame.mic1y * scale), (float)(_audioFrame.mic3x * scale), (float)(_audioFrame.mic3y * scale));

            picMap.Image = map;
            picMap.Refresh();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            dist1 = Convert.ToDouble( textBox11.Text);
            double m1x, m1y, m2x, m2y, m3x, m3y,m1db,m2db,m3db,knowndb,scale;
            double m1Sdist, m2Sdist, m3Sdist;
            bool found1, found2;
            found1 = found2 = false;
            
            m1x = Convert.ToDouble(textBox2.Text);
            m1y = Convert.ToDouble(textBox3.Text);
            m2x = Convert.ToDouble(textBox5.Text);
            m2y = Convert.ToDouble(textBox4.Text);
            m3x = Convert.ToDouble(textBox7.Text);
            m3y = Convert.ToDouble(textBox6.Text);

            m1db = Convert.ToDouble(textBox10.Text);
            m2db = Convert.ToDouble(textBox9.Text);
            m3db = Convert.ToDouble(textBox8.Text);

            knowndb = Convert.ToDouble(textBox16.Text);

            scale = Convert.ToDouble(textBox17.Text);

            //Drawing
            map = new Bitmap(600, 600);

            Font f = new Font(FontFamily.GenericSansSerif, 8);
            Brush b = new SolidBrush(Color.Blue);
            Brush r = new SolidBrush(Color.Red);
            Brush k = new SolidBrush(Color.Black);
            Graphics mapg = Graphics.FromImage(map);
            mapg.DrawString("drawing", f, b, 0, 0);
            if (knowndb != 0)
            {
                mapg.DrawEllipse(new Pen(b), (float)(m1x * scale) - 5 , (float)(m1y * scale) - 5 , 10, 10);
                mapg.DrawEllipse(new Pen(b), (float)(m2x * scale) - 5 , (float)(m2y * scale) - 5 , 10, 10);
                mapg.DrawEllipse(new Pen(b), (float)(m3x * scale) - 5 , (float)(m3y * scale) - 5 , 10, 10);

                mapg.DrawLine(new Pen(b), (float)(m1x * scale), (float)(m1y * scale), (float)(m2x * scale), (float)(m2y * scale));

                mapg.DrawLine(new Pen(b), (float)(m2x * scale), (float)(m2y * scale), (float)(m3x * scale), (float)(m3y * scale));

                mapg.DrawLine(new Pen(b), (float)(m1x * scale), (float)(m1y * scale), (float)(m3x * scale), (float)(m3y * scale));
                m1Sdist = getMfromDBs(knowndb, m1db);
                m2Sdist = getMfromDBs(knowndb, m2db);
                m3Sdist = getMfromDBs(knowndb, m3db);

                //textBox1.Text = "";
                textBox1.Text += "testing c1 and c2 ";
                textBox1.Text += findCircleIntersection(m1x, m1y, m1Sdist, m2x, m2y, m2Sdist);
                textBox1.Text += "\n";
                //textBox1.Text.Insert(textBox16.Text.Length-1, ">> trying " + knowndb + " dB");
                //textBox1.Text += "test";
                //textBox16.Text = Convert.ToString(knowndb);


                mapg.DrawEllipse(new Pen(r), (float)(((m1x) - m1Sdist) * scale), (float)(((m1y) - m1Sdist) * scale), (float)((m1Sdist * 2) * scale), (float)((m1Sdist * 2) * scale));
                mapg.DrawEllipse(new Pen(r), (float)(((m3x) - m3Sdist) * scale), (float)(((m3y) - m3Sdist) * scale), (float)((m3Sdist * 2) * scale), (float)((m3Sdist * 2) * scale));
                mapg.DrawEllipse(new Pen(r), (float)(((m2x) - m2Sdist) * scale), (float)(((m2y) - m2Sdist) * scale), (float)((m2Sdist * 2) * scale), (float)((m2Sdist * 2) * scale));

                mapg.DrawEllipse(new Pen(k), (float)(((m1x) - distFrom0to2) * scale), (float)(((m1y) - distFrom0to2) * scale), (float)((distFrom0to2 * 2) * scale), (float)((distFrom0to2 * 2) * scale));
                mapg.DrawLine(new Pen(k), (float)(this.i1x * scale), (float)(this.i1y * scale), (float)(this.i2x * scale), (float)(this.i2y * scale));
                mapg.DrawEllipse(new Pen(k), (float)(this.i1x * scale) - 5, (float)(this.i1y * scale) - 5, 10, 10);
                mapg.DrawEllipse(new Pen(k), (float)(this.i2x * scale) - 5, (float)(this.i2y * scale) - 5, 10, 10);
                mapg.DrawRectangle(new Pen(k), (float)(this.p2x * scale) - 5, (float)(this.p2y * scale) - 5, 10, 10);
                textBox1.Text += "testing c2 and c3 ";
                textBox1.Text += findCircleIntersection(m2x, m2y, m2Sdist, m3x, m3y, m3Sdist);
                textBox1.Text += "\n";
                mapg.DrawEllipse(new Pen(k), (float)(((m1x) - distFrom0to2) * scale), (float)(((m1y) - distFrom0to2) * scale), (float)((distFrom0to2 * 2) * scale), (float)((distFrom0to2 * 2) * scale));
                mapg.DrawLine(new Pen(k), (float)(this.i1x * scale), (float)(this.i1y * scale), (float)(this.i2x * scale), (float)(this.i2y * scale));
                mapg.DrawEllipse(new Pen(k), (float)(this.i1x * scale) - 5, (float)(this.i1y * scale) - 5, 10, 10);
                mapg.DrawEllipse(new Pen(k), (float)(this.i2x * scale) - 5, (float)(this.i2y * scale) - 5, 10, 10);
                mapg.DrawRectangle(new Pen(k), (float)(this.p2x * scale) - 5, (float)(this.p2y * scale) - 5, 10, 10);

 

                picMap.Image = map;
                picMap.Refresh();
            }
            else
            {
                double mindb, maxdb;
                maxdb = 150;
                mindb = m1db;
                double C1C2I1X, C1C2I2X, C2C3I1X, C2C3I2X;
                double C1C2I1Y, C1C2I2Y, C2C3I1Y, C2C3I2Y;
                //Graphics mapg = Graphics.FromImage(map);

                
                if (m2db < mindb)
                    mindb = m2db;
                if (m3db < mindb)
                    mindb = m2db;
                for (knowndb = mindb; knowndb < maxdb; knowndb += .05) //reduce the increment
                {

                   
                    mapg.FillRectangle(new SolidBrush(Color.White), 0, 0, map.Width, map.Height);

                    //Circle#Cirlce#Intersection#XorYvalue
                    C1C2I1X = C1C2I2X = C2C3I1X = C2C3I2X = 0;
                    C1C2I1Y = C1C2I2Y = C2C3I1Y = C2C3I2Y = 0;

                    r = new SolidBrush(Color.FromArgb((int)knowndb, 0,0));


                    //mapg.DrawEllipse(new Pen(b), (float)(m1x * scale) - 5, (float)(m1y * scale) - 5, 10, 10);
                    //mapg.DrawEllipse(new Pen(b), (float)(m2x * scale) - 5, (float)(m2y * scale) - 5, 10, 10);
                    //mapg.DrawEllipse(new Pen(b), (float)(m3x * scale) - 5, (float)(m3y * scale) - 5, 10, 10);

                    //mapg.DrawLine(new Pen(b), (float)(m1x * scale), (float)(m1y * scale), (float)(m2x * scale), (float)(m2y * scale));

                    //mapg.DrawLine(new Pen(b), (float)(m2x * scale), (float)(m2y * scale), (float)(m3x * scale), (float)(m3y * scale));

                    //mapg.DrawLine(new Pen(b), (float)(m1x * scale), (float)(m1y * scale), (float)(m3x * scale), (float)(m3y * scale));
                    m1Sdist = getMfromDBs(knowndb, m1db);
                    m2Sdist = getMfromDBs(knowndb, m2db);
                    m3Sdist = getMfromDBs(knowndb, m3db);
                    //textBox1.Text.Insert(textBox16.Text.Length-1, ">> trying " + knowndb + " dB");
                    textBox1.Text += "test";
                    textBox16.Text = Convert.ToString(knowndb);
                    textBox1.Text += "testing c1 and c2 ";


                    textBox1.Text += findCircleIntersection(m1x, m1y, m1Sdist, m2x, m2y, m2Sdist);
                    found1 = found;
                    textBox1.Text += "\n";
                    //set first itersection of circle 1 and circle 2
                    C1C2I1X = i1x;
                    C1C2I1Y = i1y;
                    //set second itersection of circle 1 and circle 2
                    C1C2I2X = i2x;
                    C1C2I2Y = i2y;

                    textBox1.Text += findCircleIntersection(m2x, m2y, m2Sdist, m3x, m3y, m3Sdist);
                    found2 = found;
                    textBox1.Text += "\n";

                    //set first itersection of circle 2 and circle 3
                    C2C3I1X = i1x;
                    C2C3I1Y = i1y;
                    //set second itersection of circle 2 and circle 3
                    C2C3I2X = i2x;
                    C2C3I2Y = i2y;
                    double margin = 5;
                    mapg.DrawEllipse(new Pen(k), (float)(this.i1x * scale) - 5, (float)(this.i1y * scale) - 5, 10, 10);
                    mapg.DrawEllipse(new Pen(k), (float)(this.i2x * scale) - 5, (float)(this.i2y * scale) - 5, 10, 10);
                    mapg.DrawRectangle(new Pen(k), (float)(this.p2x * scale) - 5, (float)(this.p2y * scale) - 5, 10, 10);

                    mapg.DrawEllipse(new Pen(r), (float)(((m1x) - m1Sdist) * scale), (float)(((m1y) - m1Sdist) * scale), (float)((m1Sdist * 2) * scale), (float)((m1Sdist * 2) * scale));
                    mapg.DrawEllipse(new Pen(r), (float)(((m3x) - m3Sdist) * scale), (float)(((m3y) - m3Sdist) * scale), (float)((m3Sdist * 2) * scale), (float)((m3Sdist * 2) * scale));
                    mapg.DrawEllipse(new Pen(r), (float)(((m2x) - m2Sdist) * scale), (float)(((m2y) - m2Sdist) * scale), (float)((m2Sdist * 2) * scale), (float)((m2Sdist * 2) * scale));
                    if (found1 && found2)
                    {
                        if (
                            (C1C2I1X < C2C3I1X + margin && C1C2I1X > C2C3I1X - margin &&
                            C1C2I1Y < C2C3I1Y + margin && C1C2I1Y > C2C3I1Y - margin)
                            )
                        {
                            textBox1.Text += "\n\r Found C1C2I1X == C2C3I1X && C1C2I1Y == C2C3I1Y>> DB >> " + Convert.ToString(knowndb) + "X : " + Convert.ToString(C1C2I1X) + "Y : " + Convert.ToString(C1C2I1Y);
                            picMap.Image = map;
                            picMap.Refresh();
                            break;
                        }
                        if (
                        (C1C2I1X < C2C3I2X + margin && C1C2I1X > C2C3I2X - margin
                        && C1C2I1Y < C2C3I2Y + margin && C1C2I1Y > C2C3I2Y - margin)
                            )
                        {
                            textBox1.Text += "\n\r Found C1C2I1X == C2C3I1X && C1C2I1Y == C2C3I1Y>> DB >> " + Convert.ToString(knowndb) + "X : " + Convert.ToString(C1C2I1X) + "Y : " + Convert.ToString(C1C2I1Y);
                            picMap.Image = map;
                            picMap.Refresh();
                            break;
                        }
                        if (
                        (C1C2I2X < C2C3I1X + margin && C1C2I2X > C2C3I1X - margin
                        && C1C2I2Y < C2C3I1Y + margin && C1C2I2Y > C2C3I1Y - margin)
                            )
                        {
                            textBox1.Text += "\n\r Found C1C2I1X == C2C3I1X && C1C2I1Y == C2C3I1Y>> DB >> " + Convert.ToString(knowndb) + "X : " + Convert.ToString(C1C2I1X) + "Y : " + Convert.ToString(C1C2I1Y);
                            picMap.Image = map;
                            picMap.Refresh();
                            break;
                        }
                        if (
                        (C1C2I2X < C2C3I2X + margin && C1C2I2X > C2C3I2X - margin
                        && C1C2I2Y < C2C3I2Y + margin && C1C2I2Y > C2C3I2Y - margin)
                    )
                        {
                            textBox1.Text += "\n\r Found C1C2I1X == C2C3I1X && C1C2I1Y == C2C3I1Y>> DB >> " + Convert.ToString(knowndb) + "X : " + Convert.ToString(C1C2I1X) + "Y : " + Convert.ToString(C1C2I1Y);
                            picMap.Image = map;
                            picMap.Refresh();
                            break;
                        }
                    }






                    picMap.Image = map;
                    picMap.Refresh();
                }
            }
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
           textBox15.Text = Convert.ToString( getMfromDBs(Convert.ToDouble(textBox13.Text), Convert.ToDouble(textBox14.Text)));
        }
        private string findCircleIntersection(double c1x, double c1y, double c1r, double c2x, double c2y, double c2r)
        {
            //Note: based heavily on code written by Paul Dixon found at http://sirisian.pastebin.com/f150d8f44
            found = false;
            double dist = 0;
            double distSquared = 0;
            string result = "";
            //result += "c1x =" + c1x + "c1y =" + c1y + "c2x =" + c2x + "c2y =" + c2y + "c1r =" + c1r + "c2r =" + c2r;
            double dx =  c2x - c1x;
            double dy =  c2y - c1y;
            //result += "dx =" + dx;
            //result += "dy =" + dy;

           // distSquared = Math.Pow((c2x - c1x), 2) + Math.Pow((c2y - c1y),2);
            distSquared = ((double)dx * (double)dx) + ((double)dy * (double)dy);
            dist = (double)Math.Sqrt((double)distSquared);
            //result += "Dist(r1,r2) = " + Convert.ToString(dist) + "\n";
            //result += "Dist squares = " + Convert.ToString(distSquared) + "\n";

            i1x = i1y = i2x = i2y = (double)0.0;

            
            

            if((dist ==0)&&(c1r==c2r))
            {
                result += "circles overlap\n";
                return result;
            }
           /* if(distSquared < Math.Abs(c1r-c2r)*Math.Abs(c1r-c2r))
            {
                result += "one circle contins the other";                
            }
            if (dist > c1r + c2r)
            {
                //the distance between the centers is greater than the radii, ie. too far apart
                result += "do not touch, not near";
                
            }*/
            if (dist > c1r + c2r || dist < Math.Abs(c1r - c2r))
            {
                result += "no solution\n";
                return result;
            }

            if (dist== (c1r + c2r))
            {
                
                result = "meets at one point only \n>>";
                result += "at x="+(c1x-c2x)/(c1r+c2r)*c1r+c1x + " y=" +(c1y-c2y)/(c1r+c2r)*c1r+c1y;
                return result;
               
            }
            //At this point we are assumeing there are two points of intersection
            //To find them, we must first find point 2, the point where a line connecting
            //the two points of intersection intersects with a line connecting the two radaii
            //double distFrom0to2 = ((c1r * c1r) - (c2r * c2r) + (dist * dist)) / (2 * dist);
            //double distFrom0to2 = ((c1r*c1r)-(c2r*c2r) / (2 * dist));
            distFrom0to2 = (
                
                    ((double)c1r * (double)c1r)-
                     ((double)c2r * (double)c2r) 
                    + ((double)dist * (double)dist)
                ) 
                / ((double)2.0 * (double)dist);      
            
            
            
            
            //result += "Dist to 2 = " + Convert.ToString(distFrom0to2) + "\n";
            

            //p2x = c1x+(dx*distFrom0to2/dist);
            //p2y = c1y + (dy * distFrom0to2 / dist);
            //Now we have the lenght of the base (distFrom0to2) and hypotinus(r) of a rightangle triangle we can use pythagoruses theorum
            double h = (double)Math.Sqrt(
                (c1r*c1r)-(distFrom0to2*distFrom0to2)
                );
/*
            p2x = c1x+(distFrom0to2*(c2x-c1x))/dist;
            p2y = c1y + (distFrom0to2 * (c2y - c1y)) / dist;
 */
            p2x = c1x+(dx*distFrom0to2/dist);
            p2y = c1y + (dy * distFrom0to2 / dist) ;


            //result += "h + " + h + "\n";
            //result += "p2>> X = " + p2x + " Y=" + p2y;

            //Find the offset; where the intersection point is relitive to the r1,r2 line

            double offSetX = -dy*(h/dist);
            double offSetY = dx * (h/dist);            

            //I'm assuming this is the same as v2-r for 2 vectors
            //i1x = p2x +offSetX;
            //i1y = p2y +offSetY;

            //i2x = p2x -offSetX;
           //i2y = p2y -offSetY;

            i1x = (double)p2x + (double)h * ((double)c2y - (double)c1y) / (double)dist;
            i1y = (double)p2y - (double)h * ((double)c2x - (double)c1x) / (double)dist;

            i2x = (double)p2x - (double)h * ((double)c2y - (double)c1y) / (double)dist;
            i2y = (double)p2y + (double)h * ((double)c2x - (double)c1x) / (double)dist;

            result += "found two points";
            result += "\n1>> X = " + Convert.ToString((int)i1x) + " Y=" + Convert.ToString((int)i1y) + "\n";
            result += "2>> X = " + Convert.ToString((int)i2x) + " Y=" + Convert.ToString((int)i2y+"\n");
            found = true;
            return result;

        }
        private double getMfromDBs(double dB1, double dB2)
        {
            //dB 1 sould be the source value and be higher than dB2
            double db1P,db2P,pressureChange,powerChange,dist2,sArea1,sArea2;
            dist1 = Convert.ToDouble(textBox18.Text);
            db1P = 20 * Math.Pow(10, (dB1 / 20));
            db2P = 20 * Math.Pow(10, (dB2 / 20));
            pressureChange = db1P/db2P;
            powerChange = Math.Pow(pressureChange,2);
            sArea1 = 2*3.14*Math.Pow(dist1,2);
            sArea2 = sArea1*powerChange;
            dist2 = Math.Sqrt(sArea2/(2*3.14));
            return dist2;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _audioFrame.number++;
            if (_audioFrame.number > this.maxSample)
                _audioFrame.number = 0;

            if (_audioFrame.number == 0)
                _audioFrame.highest1 = 0;

            if (_audioFrame.number == 1)
                _audioFrame.highest2 = 0;

            if (_audioFrame.number == 2)
                _audioFrame.highest3 = 0;

            if (_audioFrame.number == 3)
                _audioFrame.highest4 = 0;

            if (_audioFrame.number == 4)
                _audioFrame.highest5 = 0;

            if (_audioFrame.number == 5)
                _audioFrame.highest6 = 0;

            if (_audioFrame.number == 6)
                _audioFrame.highest7 = 0;

            if (_audioFrame.number == 7)
                _audioFrame.highest8 = 0;

            if (_audioFrame.number == 8)
                _audioFrame.highest9 = 0;

            if (_audioFrame.number == 9)
                _audioFrame.highest10 = 0;


        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}