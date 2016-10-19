﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Lokaverkefni_Johann_Gudni_Client
{
    public partial class ClientForm : Form
    {
        public ClientForm()
        {
            InitializeComponent();
        }
        private BinaryReader reader;
        private BinaryWriter writer;
        private Thread outputThread; 
        private TcpClient connection; 
        private NetworkStream stream;        
        private bool done = false;
        private TextBox tb_textGuess;
        private Label lb_question;
        private RadioButton[] rd_buttonGuess;
        private string current_question;
        private int score;
        private string ipaddress;
        private int question_Type;
        

        private void ClientForm_Load(object sender, EventArgs e)
        {
            using(var ipForm = new chooseIP())
            {
                var result = ipForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    ipaddress = ipForm.ipaddress;

                }
                else
                {
                    ipaddress = "127.0.0.1";
                }
            }
            

            
            tb_textGuess = new TextBox();
            tb_textGuess.Location = new Point(200, 300);


            lb_question = new Label();
            tb_textGuess.Location = new Point(200, 250);

            try
            {
                connection = new TcpClient(ipaddress, 50000);
                stream = connection.GetStream();
                writer = new BinaryWriter(stream);
                reader = new BinaryReader(stream);

                outputThread = new Thread(new ThreadStart(Run));
                outputThread.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("Server Is Currently Down! Please Try Again Later.");
                Environment.Exit(0);
            }

        }

        
        private delegate void DisplayTextDelegate(string message);

        private void DisplayMessage(string message)
        {
            if (rtb_output.InvokeRequired)
            {
                Invoke(new DisplayTextDelegate(DisplayMessage),
                   new object[] { message });
            }
            else
                rtb_output.Text += message + "\n";
        }


        private delegate void DisplayLabelDelegate(string question);

        private void DisplayLabel(string question)
        {
            if (rtb_output.InvokeRequired)
            {
                Invoke(new DisplayLabelDelegate(DisplayLabel),
                   new object[] { question });
            }
            else
                lb_question.Text = question;
        }

        private delegate void DisplayRadioDelegate(string answer, int i);

        private void DisplayRadio(string answer, int i)
        {
            if (rd_buttonGuess[i].InvokeRequired)
            {
                Invoke(new DisplayRadioDelegate(DisplayRadio),
                   new object[] { answer, i});
            }
            else
                rd_buttonGuess[i].Text = answer;
            this.Controls.Add(rd_buttonGuess[i]);

        }

        private delegate void DisplayTextBoxDelegate(string answer, TextBox tb_answer);

        private void DisplayTextBox(string answer, TextBox tb_answer)
        {
            if (tb_answer.InvokeRequired)
            {
                Invoke(new DisplayTextBoxDelegate(DisplayTextBox),
                   new object[] { answer, tb_answer });
            }
            else
                tb_answer.Text = answer;
            this.Controls.Add(tb_answer);

        }

        public void ProcessMessage(string message)
        {
            
            if (message.Split('|').Length > 1)
            {
                
                //This is a Question
                current_question = message.Split('|')[0];
                MessageBox.Show("Got the Question");
                question_Type = Convert.ToInt32(message.Split('|')[1]);
                switch(question_Type)
                {
                    
                    case 0:
                        DisplayLabel(current_question);
                        this.Invoke((MethodInvoker)(() => Controls.Add(lb_question)));
                        tb_textGuess = new TextBox();
                        DisplayTextBox(message.Split('|')[3], tb_textGuess);

                        break;

                    case 1:
                        DisplayLabel(current_question);
                        
                        rd_buttonGuess = new RadioButton[message.Split('|').Length-3];
                        this.Invoke((MethodInvoker)(() => Controls.Add(lb_question)));
                        for (int i = 3; i < message.Split('|').Length; i++)
                        {
                            string text = message.Split('|')[i];
                            DisplayRadio(text, i-3);
                            
                        }
                        

                        break;

                    case 2:
                        

                        break;


                }
                    

            }

            if (message == "Win")
            {
                MessageBox.Show("Takk Fyrir Leikinn");


                Environment.Exit(0);
            }
            else if (message.Split()[0] == "Lose")
            {
                MessageBox.Show("You lost, The correct word is " + message.Split()[1]);


                Environment.Exit(0);

            }
            else
            {
                DisplayMessage(message);
            }
      
               
        }

       



        
        
        

        public void Run()
        {

            try
            {
                while (!done)
                    ProcessMessage(reader.ReadString());
            }
            catch (IOException)
            {
                MessageBox.Show("Server Is Down, Game Over!!", "Error",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                done = true;
                System.Environment.Exit(System.Environment.ExitCode);
            }
            catch (Exception)
            {

                Environment.Exit(0);
            }
            
        }

        private void bt_guess_Click_1(object sender, EventArgs e)
        {
            writer.Write(tb_textGuess.Text.ToLower());
            tb_textGuess.Clear(); 
        }

        

    }
}
