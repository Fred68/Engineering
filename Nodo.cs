using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;						// Per lettura e scrittura su file

using Fred68.Tools.Matematica;
using Fred68.Tools.Utilita;

namespace Fred68.Tools.Engineering
	{
	class Nodo : Oggetto
		{
		protected Point2D posizione;
		protected static readonly new string descrittore = "NODO";
		#region PROPRIETA
		public Point2D Posizione
			{
			get {return posizione;}
			set {posizione = value;}
			}
		#endregion
		#region COSTRUTTORI
		public Nodo() : base()
			{
			posizione = new Point2D();
			selezionato = false;
			tipo = TipoOggetto.Nodo;
			collegamenti = 0;
			}
		public Nodo(Point2D puntoNodo, int numeroNodo, string nomeNodo) : base(numeroNodo, nomeNodo)
			{
			posizione = puntoNodo;
			selezionato = false;
			tipo = TipoOggetto.Nodo;
			collegamenti = 0;
			}
		public Nodo(Point2D puntoNodo, int numeroNodo) : base(numeroNodo, NomeStandard)
			{
			posizione = puntoNodo;
			selezionato = false;
			tipo = TipoOggetto.Nodo;
			collegamenti = 0;
			}
		public Nodo(Point2D puntoNodo) : base()
			{
			posizione = puntoNodo;
			selezionato = false;
			tipo = TipoOggetto.Nodo;
			collegamenti = 0;
			}
		#endregion
		#region FUNZIONI
		#endregion
		#region IO SU STREAM
		public new bool Scrivi(StreamWriter sw)
			{
			sw.Write(descrittore); sw.Write(separatore);
			sw.Write(nID); sw.Write(separatore);
			sw.Write(nome); sw.Write(separatore);
			sw.Write(numero); sw.Write(separatore);
			sw.Write(posizione.x); sw.Write(separatore);
			sw.Write(posizione.y); sw.Write(separatore);
			sw.WriteLine();
			return true;
			}
		public new bool Leggi(StreamReader sr)
			{
			string str;									// Riga letta dal file
			int i;										// Contatore
			int itmp;									// Temporanei per conversione
			double dtmp;
			TokenString tk = new TokenString();			// Tokenizzatore
			if (!sr.EndOfStream)
				{
				str = sr.ReadLine();					// Legge una riga (ossia un oggetto completo)
				tk.Set(ref str, "\t ");					// Imposta il tokenizzatore, str per reference
				i = 0;									// Azzera contatore
				foreach (string s in tk)
					{
					switch (i)
						{
						case 0:
							if (s != descrittore)		// Se oggetto non riconosciuto
								i = int.MaxValue;
							break;
						case 1:
							if (int.TryParse(s, out itmp))
								nID = itmp;
							break;
						case 2:
							nome = s;
							break;
						case 3:
							if (int.TryParse(s, out itmp))
								numero = itmp;
							break;
						case 4:
							if (double.TryParse(s, out dtmp))
								posizione.x = dtmp;
							break;
						case 5:
							if (double.TryParse(s, out dtmp))
								posizione.y = dtmp;
							break;
						}
					i++;
					}
				}
			return true;
			}
		#endregion
		}
	}
