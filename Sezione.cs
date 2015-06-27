using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;						// Per lettura e scrittura su file
using Fred68.Tools.Utilita;				// Per TokenString


namespace Fred68.Tools.Engineering
	{
	enum Sezioni {Nulla, Utente, Tonda, Tubolare, Rettangolare, RettangolareCava, aT, doppioT, NumeroSezioni};
	class Sezione : Oggetto
		{
		protected static readonly new string descrittore = "SEZ";		// Descrittore
		protected static readonly int Np = 10;							// Numero di parametri
		public double Jxx;												// Modulo di inerzia a flessione
		public double Jyy;												// Modulo di inerzia a flessione
		public double A;												// Area
		public double tx;
		public double ty;
		protected int sezione;
		protected double[] p = new double[Np];							// I parametri
		#region COSTRUTTORI
		public Sezione() : base()
			{
			Jxx = 0.0;
			Jyy = 0.0;
			A = 0.0;
			tx = 1.0;
			ty = 1.0;
			tipo = TipoOggetto.Sezione;
			sezione = (int)Sezioni.Utente;
			for(int i=0; i < Np; i++)
				p[i] = (double)(i+10);
			}
		#endregion
		#region FUNZIONI
		bool Get(int i, double val)										// Accesso ai valori dei parametri numerati
			{
			if ((i >= 0) && (i < Np))
				{
				val = p[i];
				return true;
				}
			else
				return false;
			}
		bool Set(int i, double val)
			{
			if( (i >= 0) && (i < Np) )
				{
				p[i] = val;
				return true;
				}
			else
				return false;
			}
		virtual public bool Calcola()		// Funzione di calcolo 
			{
			Jxx = (p[0]*Math.Pow(p[1],3))/12;
			Jyy = (p[1] * Math.Pow(p[0], 3)) / 12;
			A = p[0] * p[1];
			return true;
			}
		#endregion
		#region IO SU STREAM
		public new bool Scrivi(StreamWriter sw)
			{
			sw.Write(descrittore); sw.Write(separatore);		// Scrive i dati
			sw.Write(nID); sw.Write(separatore);
			sw.Write(nome); sw.Write(separatore);
			sw.Write(numero); sw.Write(separatore);				// Non scrive il materiale di base
			sw.Write(A); sw.Write(separatore);
			sw.Write(Jxx); sw.Write(separatore);
			sw.Write(Jyy); sw.Write(separatore);
			sw.Write(tx); sw.Write(separatore);
			sw.Write(ty); sw.Write(separatore);
			sw.Write(sezione); sw.Write(separatore);
			for(int i=0; i<Np; i++)
				{
				sw.Write(p[i]);
				sw.Write(separatore);
				}
			sw.WriteLine();
			return true;
			}
		public new bool Leggi(StreamReader sr)
			{
			string str;									// Riga letta dal file
			int i, ip;									// Contatori
			int itmp = 0;								// Temporanei per conversione
			double dtmp = 0.0;
			TokenString tk = new TokenString();			// Tokenizzatore
			if (!sr.EndOfStream)
				{
				str = sr.ReadLine();					// Legge una riga (ossia un oggetto completo)
				tk.Set(ref str, "\t ");					// Imposta il tokenizzatore, str per reference
				i = 0;									// Azzera contatori
				ip = 0;
				foreach (string s in tk)
					{
					switch (i)
						{
						case 0:								// Legge i dati in ordine
							if (s != descrittore)				// Descrittore
								i = int.MaxValue;
							break;
						case 1:									// ID
							if (int.TryParse(s, out itmp))
								nID = itmp;
							break;
						case 2:									// Nome
							nome = s;
							break;
						case 3:									// Numero
							if (int.TryParse(s, out itmp))
								numero = itmp;
							break;
						case 4:									// Legge i dati
							if (double.TryParse(s, out dtmp))
								A = dtmp;								
							break;										
						case 5:
							if (double.TryParse(s, out dtmp))		
								Jxx = dtmp;
							break;
						case 6:										
							if (double.TryParse(s, out dtmp))
								Jyy = dtmp;
							break;
						case 7:										
							if (double.TryParse(s, out dtmp))
								tx = dtmp;
							break;
						case 8:										
							if (double.TryParse(s, out dtmp))
								ty = dtmp;
							break;
						case 9:
							if (int.TryParse(s, out itmp))
								sezione = itmp;
							break;
						default:
							if (i < Np + 9)						// Oltre i dati di base, legge i parametri usando l'indice ip
								{
								if (double.TryParse(s, out dtmp))		
									{
									p[ip] = dtmp;
									ip++;
									}
								}
							break;
						}	// Fine dello swtch
					i++;
					}	// Fine del ciclo foreach
				}
			return true;
			}
		#endregion

		}
	}
