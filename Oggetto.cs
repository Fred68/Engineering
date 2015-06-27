using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fred68.Tools.Utilita;

namespace Fred68.Tools.Engineering
	{
	enum TipoOggetto { Nullo, Nodo, Trave, Materiale, Sezione, BC };

	class Oggetto
		{
		protected static readonly int nonvalido = -1;
		protected static readonly string nomestandard = "-";
		protected static readonly int primoindice = 0;
		protected static readonly int numerostandard = 1;
		protected static readonly string separatore = "\t";
		protected static readonly string descrittore = "OGGETTO"; 
		public static int NonValido { get { return nonvalido; } }
		public static string NomeStandard { get {return nomestandard; } }
		public static int NumeroStandard { get { return numerostandard; } }
		public static int PrimoIndice { get { return primoindice;} }

		protected string nome;									// Nome
		protected int nID;										// ID, non modificabile dall'utente
		protected int numero;									// Numero, modificabile
		protected bool selezionato;
		protected TipoOggetto tipo = TipoOggetto.Nullo;
		protected int collegamenti;
		#region PROPRIETA
		public string Nome
			{
			get {return nome;}
			set {nome = value;}
			}
		public int ID
			{
			get {return nID;}
			set {nID = value;}
			}
		public int Numero
			{
			get { return numero; }
			set { numero = value; }
			}
		public string ID_Nome
			{
			get {return (nID.ToString()+' '+nome);}
			}
		public TipoOggetto Tipo
			{
			get {return tipo;}
			}
		public int Collegamenti
			{
			get { return collegamenti; }
			}
		public bool Collegato
			{
			get { return collegamenti > 0; }
			}
		#endregion
		#region COSTRUTTORI
		public Oggetto(int ID, string Nome)
			{
			nome = Nome;
			nID = ID;
			selezionato = false;
			numero = numerostandard;
			}
		public Oggetto()
			{
			nome = nomestandard;
			nID = nonvalido;
			selezionato = false;
			numero = numerostandard;
			}
		#endregion
		#region FUNZIONI
		public void Seleziona()
			{
			selezionato = true;
			}
		public void SelezionaDeseleziona()
			{
			if(selezionato)
				selezionato = false;
			else
				selezionato = true;
			}
		public void Deseleziona()
			{
			selezionato = false;
			}
		public bool Selezionato
			{
			get {return selezionato;}
			}
		public int AggiungiCollegamento()
			{
			collegamenti++;
			return collegamenti;
			}
		public int RimuoviCollegamento()
			{
			collegamenti--;
			if (collegamenti < 0)
				collegamenti = 0;
			return collegamenti;
			}
		public override string ToString()
			{
			return ID.ToString()+'\n'+ Numero.ToString() +'\n'+Nome;
			}
		public virtual bool Associa(Oggetto ogg)				// Associa ad un altro oggetto
			{
			return false;
			}
		public virtual bool Dissocia(TipoOggetto tipogg)		// Dissocia da un tipo di oggetto
			{
			return false;
			}
		public virtual bool Dissocia(Oggetto ogg)				// Dissocia da un altro oggetto specifico
			{
			return false;
			}
		public virtual bool Dissocia()							// Dissocia dall'oggetto cui era associato
			{
			return false;
			}
		public virtual bool Associato()							// Indica se l'oggetto e` associato o no a qualche altro oggetto
			{													// Non considera i nodi delle travi. Valido per: vd. Note.txt
			return false;
			}
		#endregion
		#region IO SU STREAM
		public bool Scrivi(StreamWriter sw)
			{
			sw.Write(descrittore); sw.Write(separatore);
			sw.Write(nID); sw.Write(separatore);
			sw.Write(nome); sw.Write(separatore);
			sw.Write(numero); sw.Write(separatore);
			sw.WriteLine();
			return true;
			}
		public bool Leggi(StreamReader sr)
			{
			string str;									// Riga letta dal file
			int i;										// Contatore
			int itmp;									// Temporaneo per conversione
			TokenString tk = new TokenString();			// Tokenizzatore
			if(!sr.EndOfStream)
				{
				str = sr.ReadLine();					// Legge una riga (ossia un oggetto completo)
				tk.Set(ref str, "\t ");					// Imposta il tokenizzatore, str per reference
				i = 0;									// Azzera contatore
				foreach(string s in tk)
					{
					switch(i)
						{
						case 0:
							if(s != descrittore)		// Se oggetto non riconosciuto
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
						}
					i++;
					}
				} 
			return true;
			}
		#endregion
		}
	}
