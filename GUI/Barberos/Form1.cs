using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Barberos
{
    public partial class Form1 : Form
    {
        Thread[] threadsBarberos = new Thread[5];

        private int tiempoDeCorte;
        private int numBarberos = 1, clientesEsperando = 0;
        private PictureBox[] picBarbero;
        private PictureBox[] picClienteEsperando;
        private PictureBox[] picClienteAtendido;
        private ProgressBar[] barCorte;
        private bool[] barberoDespedido;

        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 5; i++)
            {
                threadsBarberos[i] = new Thread(new ParameterizedThreadStart(StBarbero_Cortando));
            }
            picBarbero = new PictureBox[]
            {
                picBarbero1,
                picBarbero2,
                picBarbero3,
                picBarbero4,
                picBarbero5
            };
            picClienteEsperando = new PictureBox[]
            {
                picCliente_Esperando1,
                picCliente_Esperando2,
                picCliente_Esperando3,
                picCliente_Esperando4,
                picCliente_Esperando5
            };
            picClienteAtendido = new PictureBox[]
            {
                picCliente_Satisfecho1,
                picCliente_Satisfecho2,
                picCliente_Satisfecho3,
                picCliente_Satisfecho4,
                picCliente_Satisfecho5
            };

            barCorte = new ProgressBar[]
            {
                Corte1,
                Corte2,
                Corte3,
                Corte4,
                Corte5
            };

            barberoDespedido = new bool[5];

            tiempoDeCorte = 1000;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            bool clienteAtendido = false;
            for (int i = 0; i < numBarberos; i++)
            {
                if (!threadsBarberos[i].IsAlive)
                {
                    clienteAtendido = true;
                    var v = new { form = this, index = i };
                    if (threadsBarberos[i].ThreadState == ThreadState.Stopped)
                    {
                        threadsBarberos[i] = new Thread(new ParameterizedThreadStart(StBarbero_Cortando));
                    }
                    threadsBarberos[i].Start(v);
                    break;
                }
            }
            if (!clienteAtendido && clientesEsperando < 5)
            {
                picClienteEsperando[clientesEsperando].Image = Properties.Resources.Cliente;
                Clientes.Text = (++clientesEsperando).ToString();
            }
        }

        private void Tiempo_Scroll(object sender, EventArgs e)
        {
            tiempoDeCorte = Tiempo.Value * 1000;
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > numBarberos)
            {
                while (numBarberos < numericUpDown1.Value)
                {
                    lock (this)
                    {
                        if (clientesEsperando > 0 && !threadsBarberos[numBarberos].IsAlive)
                        {
                            Clientes.Text = (--clientesEsperando).ToString();
                            picClienteEsperando[clientesEsperando].Image
                                = Properties.Resources.Silla_de_espera;

                            var v = new { form = this, index = numBarberos };
                            if (threadsBarberos[numBarberos].ThreadState == ThreadState.Stopped)
                            {
                                threadsBarberos[numBarberos] =
                                    new Thread(new ParameterizedThreadStart(StBarbero_Cortando));
                            }
                            threadsBarberos[numBarberos].Start(v);
                        }
                    }

                    barberoDespedido[numBarberos] = false;
                    picBarbero[numBarberos++].Image = Properties.Resources.Barbera;
                }
            }
            else if (numericUpDown1.Value < numBarberos)
            {
                while(numBarberos > numericUpDown1.Value)
                {
                    barberoDespedido[--numBarberos] = true;
                    if(!threadsBarberos[numBarberos].IsAlive)
                    {
                        picBarbero[numBarberos].Image = null;
                    }
                }
            }
        }

        private static T Cast<T>(T typeHolder, Object x)
        {
            // typeHolder above is just for compiler magic
            // to infer the type to cast x to
            return (T)x;
        }

        static void StBarbero_Cortando(object anon)
        {
            var a = new { form = (Form1)null, index = 0 };
            a = Cast(a,anon);
            a.form.Barbero_Cortando(a.index);
        }

        void Barbero_Cortando(int index)
        {
            bool atendiendoCliente = true;

            try
            {
                do
                {
                    int tiempo = 0;
                    Invoke(new Action(() =>
                    {
                        picBarbero[index].Image = Properties.Resources.Barbera_cortando;
                        picClienteAtendido[index].Image = Properties.Resources.Cliente;
                        barCorte[index].Maximum = tiempoDeCorte;
                        barCorte[index].Value = 0;
                    }));

                    while (tiempo < barCorte[index].Maximum)
                    {
                        Thread.Sleep(1000);
                        tiempo += 1000;
                        Invoke(new Action(() =>
                        {
                            barCorte[index].Value = tiempo;
                        }));
                    }

                    Invoke(new Action(() =>
                    {
                        picBarbero[index].Image = Properties.Resources.Barbera;
                        picClienteAtendido[index].Image = Properties.Resources.Cliente_Satisfecho;
                    }));

                    Thread.Sleep(1000);

                    lock (this)
                    {
                        if (clientesEsperando > 0 && !barberoDespedido[index])
                        {
                            Invoke(new Action(() =>
                            {
                                Clientes.Text = (--clientesEsperando).ToString();
                                picClienteEsperando[clientesEsperando].Image
                                    = Properties.Resources.Silla_de_espera;
                            }));
                        }
                        else atendiendoCliente = false;
                    }
                }
                while (atendiendoCliente && !barberoDespedido[index]);

                Invoke(new Action(() =>
                {
                    picClienteAtendido[index].Image = null;
                    barCorte[index].Value = 0;
                    if (barberoDespedido[index])
                    {
                        picBarbero[index].Image = null;
                    }
                }));
            }
            catch(InvalidOperationException e)
            {
                if (!this.IsDisposed) throw e;
            }
        }
    }
}
