using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;						// Per lettura e scrittura su file
using Fred68.Tools.Utilita;

using Fred68.Tools.Matematica;

namespace Fred68.Tools.Engineering
	{
	class Trave : Oggetto																// Trave, derivato da Oggetto
		{
		class RifTrave																	// Classe interna, usata per la memorizzazione
			{																			// degli nID durante la lettura da file
			public int[] nodi;															// Numeri degli ID degli oggetti cui fa riferimento...
			public int sezione;															// ...la trave. Usato solo per input da file.
			public int materiale;
			public RifTrave()															// Locale, solo per input da file
				{
				nodi = new int[Trave.NumeroNodi];
				for(int i=0; i < Trave.NumeroNodi; i++)
					nodi[i] = Oggetto.NonValido;
				sezione = Oggetto.NonValido;
				materiale = Oggetto.NonValido;
				}
			}
		public static readonly int NumeroNodi = 2;										// Numero di nodi dell'elemento
		protected static readonly new string descrittore = "TRAVE";
		protected Nodo[] nodi;
		protected Sezione sezione = null;
		protected Materiale materiale = null;
		RifTrave rifTrave;
		#region COSTRUTTORI
		public Trave()	: base()
			{
			nodi = new Nodo[NumeroNodi];
			selezionato = false;
			tipo = TipoOggetto.Trave;
			nodi[0] = new Nodo();
			nodi[1] = new Nodo();
			nodi[0].AggiungiCollegamento();
			nodi[1].AggiungiCollegamento();
			sezione = new Sezione();
			sezione.AggiungiCollegamento();
			materiale = new Materiale();
			materiale.AggiungiCollegamento();
			rifTrave = null;
			}
		public Trave(Nodo n1, Nodo n2)	: base()
			{
			nodi = new Nodo[NumeroNodi];
			selezionato = false;
			nodi[0] = n1;
			nodi[1] = n2;
			tipo = TipoOggetto.Trave;
			n1.AggiungiCollegamento();
			n2.AggiungiCollegamento();
			sezione = new Sezione();
			sezione.AggiungiCollegamento();
			materiale = new Materiale();
			materiale.AggiungiCollegamento();
			rifTrave = null;
			}
		public Trave(Nodo n1, Nodo n2, int numeroTrave, string nomeTrave) : base(numeroTrave, nomeTrave)
			{
			nodi = new Nodo[2];
			selezionato = false;
			nodi[0] = n1;
			nodi[1] = n2;
			tipo = TipoOggetto.Trave;
			n1.AggiungiCollegamento();
			n2.AggiungiCollegamento();
			sezione = new Sezione();
			sezione.AggiungiCollegamento();
			materiale = new Materiale();
			materiale.AggiungiCollegamento();
			rifTrave = null;
			}
		#endregion
		#region PROPRIETA
		public Point2D Posizione												// Restituisce la posizione (punto medio)
			{
			get { return (nodi[0].Posizione + nodi[1].Posizione) / 2.0; }
			}
		public Sezione Sezione													// Restituisce la sezione
			{
			get {return sezione; }
			set {sezione = value; }
			}
		public Materiale Materiale												// Restituisce il materiale
			{
			get {return materiale; }
			set {materiale = value; }
			}
		#endregion
		#region FUNZIONI
		public Nodo Nodo(int numNodo)											// Restituisce il nodo (1' o 2')
			{
			if ((numNodo >= 0) && (numNodo < NumeroNodi))
				{
				return nodi[numNodo];
				}
			else
				{
				return new Nodo();
				}
			}
		public Point2D GetPoint2D(int i)										// Restituisce la posizone del nodo (1' o 2')
			{
			if( (i >= 0) && (i < NumeroNodi))
				{
				return nodi[i].Posizione;
				}
			else
				{
				return new Point2D(MatrixInfo.NullMatrix);
				}
			}
		public bool SetNodo(int i, Nodo n)										// Imposta il nodo
			{
			if( (i >= 0) && (i < NumeroNodi))
				{
				nodi[i] = n;
				nodi[i].AggiungiCollegamento();
				return true;
				}
			else
				{
				return false;
				}
			}
		public bool SostituisciIndici(ArchivioStruttura arch)					// Sostituisce gli indici con i riferimenti
			{
			bool fatto = false;
			if(rifTrave != null)
				{
				fatto = true;															// Mette a true; se errore qulunque: false
				if(rifTrave.nodi[0] != Oggetto.NonValido)								// Controlla se indice inizializzato. Se no...
					{																	// Prosegue senza errori
					Nodo n = (Nodo)arch.Trova(TipoOggetto.Nodo, rifTrave.nodi[0]);
					if(n != null)
						{
						nodi[0] = n;
						nodi[0].AggiungiCollegamento();
						}
					else
						fatto = false;
					}
				if (rifTrave.nodi[1] != Oggetto.NonValido)
					{
					Nodo n = (Nodo)arch.Trova(TipoOggetto.Nodo, rifTrave.nodi[1]);
					if (n != null)
						{
						nodi[1] = n;
						nodi[1].AggiungiCollegamento();
						}
					else
						fatto = false;
					}
				if (rifTrave.sezione != Oggetto.NonValido)
					{
					Sezione s = (Sezione)arch.Trova(TipoOggetto.Sezione, rifTrave.sezione);
					if (s != null)
						{
						sezione = s;
						sezione.AggiungiCollegamento();
						}
					else
						fatto = false;
					}
				if (rifTrave.materiale != Oggetto.NonValido)
					{
					Materiale m = (Materiale)arch.Trova(TipoOggetto.Materiale, rifTrave.materiale);
					if (m != null)
						{
						materiale = m;
						materiale.AggiungiCollegamento();
						}
					else
						fatto = false;
					}
				}
			if(fatto)											// Se operazione completata, cancella il riferimento al_
				rifTrave = null;								// l'oggetto con gli indici (eliminato poi dal gc)
			return fatto;
			}
		public override bool Associa(Oggetto ogg)								// Associa ad un oggetto (materiale o sezione)
			{
			bool ok = false;
			switch(ogg.Tipo)
				{
				case TipoOggetto.Materiale:
					{
					if(this.materiale != null)									// Se l'oggetto era gia` associato ad un materiale...
						(this.materiale).RimuoviCollegamento();					// decrementa i link del vecchio materiale
					Materiale = (Materiale) ogg;								// Imposta il riferimento al nuovo materiale
					ogg.AggiungiCollegamento();									// incrementa i link del nuovo materiale
					ok = true;
					break;
					}
				case TipoOggetto.Sezione:
					{
					if (this.sezione != null)									// Se l'oggetto era gia` associato ad una sezione...
						(this.sezione).RimuoviCollegamento();					// decrementa i link della vecchia sezione
					Sezione = (Sezione)ogg;										// Imposta il riferimento alla nuova sezione
					ogg.AggiungiCollegamento();									// incrementa i link del nuovo materiale
					ok = true;
					break;
					}
				}
			return ok;
			}
		public override bool Dissocia(TipoOggetto tipogg)						// Dissocia da un tipo di oggetto
			{
			bool ok = false;
			switch (tipogg)
				{
				case TipoOggetto.Materiale:
						{
						if (this.materiale != null)							// Se l'oggetto era gia` associato ad un materiale...
							(this.materiale).RimuoviCollegamento();			// decrementa i link del vecchio materiale
						Materiale = (Materiale)null;						// Cancella il riferimento al materiale
						ok = true;
						break;
						}
				case TipoOggetto.Sezione:
						{
						if (this.sezione != null)							// Se l'oggetto era gia` associato ad una sezione...
							(this.sezione).RimuoviCollegamento();			// decrementa i link della vecchia sezione
						Sezione = (Sezione)null;							// Cancella il riferimento alla sezione
						ok = true;
						break;
						}
				}
			return ok;
			}
		public override bool Associato()										// Restituisce true se all'oggetto ne e` associato un altro
			{															
			if((this.materiale != null) || (this.sezione != null))
				return true;
			return false;
			}
		#endregion
		#region IO SU STREAM
		public new bool Scrivi(StreamWriter sw)
			{
			sw.Write(descrittore); sw.Write(separatore);
			sw.Write(nID); sw.Write(separatore);
			sw.Write(nome); sw.Write(separatore);
			sw.Write(numero); sw.Write(separatore);
			sw.Write(nodi[0].ID); sw.Write(separatore);
			sw.Write(nodi[1].ID); sw.Write(separatore);
			sw.Write(sezione.ID); sw.Write(separatore);
			sw.Write(materiale.ID); sw.Write(separatore);
			sw.WriteLine();
			return true;
			}
		public new bool Leggi(StreamReader sr)
			{
			string str;									// Riga letta dal file
			int i;										// Contatore
			int itmp;									// Temporanei per conversione
			rifTrave = new RifTrave();
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
							if (int.TryParse(s, out itmp))
								rifTrave.nodi[0] = itmp;
							break;
						case 5:
							if (int.TryParse(s, out itmp))
								rifTrave.nodi[1] = itmp;
							break;
						case 6:
							if (int.TryParse(s, out itmp))
								rifTrave.sezione = itmp;
							break;
						case 7:
							if (int.TryParse(s, out itmp))
								rifTrave.materiale = itmp;
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
