using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;										// Per lettura e scrittura su file
//using Fred68.Tools.Matematica;
using Fred68.Tools.Utilita;

namespace Fred68.Tools.Engineering
	{

	enum TipoBC
		{		NonDefinito,											// Tipo di condizione al contorno
				ForzaNodale,
				VincoloNodale,
				CaricoTrave,
				TermicoTrave,
				ForzaTrave };
	enum Gdl {x = 0, y = 1, z = 2, N = 3};								// Indici dei gradi di liberta` e numero totale

	class BC	:	Oggetto												// BC Boundary condition, derivara da Oggetto
		{
		class RifOggetto												// Classe interna usata per la memorizzazione
			{
			public int oggetto;
			public TipoOggetto tipo;
			public RifOggetto()
				{
				oggetto = Oggetto.NonValido;
				tipo = TipoOggetto.Nullo;
				}
			}
		protected static readonly new string descrittore = "BC";		// Descrittore per IO
		protected TipoBC tipoBC;										// Tipo di condizione al contorno
		protected Oggetto ogg;											// Oggetto (nodo o trave) a cui e` applicato
		protected int nogg;												// e relativo numero (solo per il load)
		protected bool[] gdlAttivo;										// Quali dei gdl (forza o cedimento) e` attivo
		protected double[] val;											// Valori delle condizioni al contorno (cedimento, forza o altro).
		RifOggetto rifOggetto;											

		#region PROPRIETA
		public TipoBC TipoBC
			{
			get {return tipoBC;}
			set {tipoBC = value;}
			}
		#endregion

		#region COSTRUTTORI
		public BC() : base()											// Costruisce i BC ed alloca gli array
			{
			selezionato = false;
			tipo = TipoOggetto.BC;
			collegamenti = 0;
			gdlAttivo = new bool[(int)Gdl.N];
			val = new double[(int)Gdl.N];
			tipoBC = TipoBC.NonDefinito;
			ogg = (Oggetto) null;
			nogg = 0;
			}
		public BC(int numeroBC, string nomeBC) : base(numeroBC, nomeBC)
			{
			selezionato = false;
			tipo = TipoOggetto.BC;
			collegamenti = 0;
			gdlAttivo = new bool[(int)Gdl.N];
			val = new double[(int)Gdl.N];
			tipoBC = TipoBC.NonDefinito;
			ogg = (Oggetto)null;
			nogg = 0;
			}
		#endregion

		#region FUNZIONI
		public override bool Associa(Oggetto ogg)						// Associa il BC ad un oggetto (materiale o sezione)
			{
			bool ok = false;
			switch (ogg.Tipo)
				{
				case TipoOggetto.Nodo:
						{
						if ( (tipoBC == TipoBC.ForzaNodale) ||					// Verifica se tipo di vincolo corretto	
							 (tipoBC == TipoBC.VincoloNodale) )
							{
							if (this.ogg != null)								// Se il BC era gia` associato ad un oggetto...
								(this.ogg).RimuoviCollegamento();				// decrementa i link del vecchio oggetto
							this.ogg = ogg;										// Imposta il riferimento al nuovo oggetto
							(this.ogg).AggiungiCollegamento();					// incrementa i link del nuovo oggetto
							ok = true;
							
							}
						break;
						}
				case TipoOggetto.Trave:
						{
						if ( (tipoBC == TipoBC.CaricoTrave) ||					// Verifica se tipo di vincolo corretto	
							 (tipoBC == TipoBC.ForzaTrave) ||
							 (tipoBC == TipoBC.TermicoTrave) )
							{
							if (this.ogg != null)								// Se il BC era gia` associato ad un oggetto...
								(this.ogg).RimuoviCollegamento();				// decrementa i link del vecchio oggetto
							this.ogg = ogg;										// Imposta il riferimento al nuovo oggetto
							(this.ogg).AggiungiCollegamento();					// incrementa i link del nuovo materiale
							ok = true;
							}
						break;
						}
				}
			return ok;
			}
		public override bool Dissocia()									// Dissocia il BC dall'oggetto cui era associato
			{
			bool ok = true;
			if(ogg != null)														// Se aveva il riferimento ad un oggetto...
				{
				ogg.RimuoviCollegamento();										// ...riduce i collegamenti dell'oggetto e...
				ogg = (Oggetto) null;											// ...elimina il riferimento
				}
			return ok;
			}
		public override bool Associato()								// Restituisce true se all'oggetto ne e` associato un altro
			{
			if (this.ogg != null)
				return true;
			else
				return false;
			}
		#endregion

		#region IO SU STREAM
		public new bool Scrivi(StreamWriter sw)
			{
			int i;															// contatore
			sw.Write(descrittore); sw.Write(separatore);
			sw.Write(nID); sw.Write(separatore);
			sw.Write(nome); sw.Write(separatore);
			sw.Write(numero); sw.Write(separatore);
			sw.Write(tipoBC.ToString()); sw.Write(separatore);				// Tipo di condizione al contorno
			sw.Write(ogg.ID); sw.Write(separatore);							// ID dell'oggetto cui e` applicato
			for(i=0; i < (int)Gdl.N; i++)
				{
				sw.Write(gdlAttivo[i].ToString()); sw.Write(separatore);	// Scrive quali sono i gdl bloccati o caricato	
				}
			for (i = 0; i < (int)Gdl.N; i++)
				{
				sw.Write(val[i]); sw.Write(separatore);						// Scrive i valori dei cedimenti imposti o delle forze
				}
			sw.WriteLine();
			return true;
			}
		public new bool Leggi(StreamReader sr)
			{
			string str;									// Riga letta dal file
			int i;										// Contatore
			int itmp;									// Temporanei per conversione
			rifOggetto = new RifOggetto();
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
						case 4:									// Tipo
							//if (int.TryParse(s, out itmp))
								//rifTrave.nodi[0] = itmp;
							break;
						case 5:
							//if (int.TryParse(s, out itmp))		// num. oggetto
								//rifTrave.nodi[1] = itmp;
							break;
						case 6:
							//if (int.TryParse(s, out itmp))		// i 3 bool
								//rifTrave.sezione = itmp;
							break;
						case 7:
							//if (int.TryParse(s, out itmp))		// i tre float
								//rifTrave.materiale = itmp;
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
