using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Fred68.Tools.Matematica;
using Fred68.Tools.Grafica;
using Fred68.Tools.Utilita;
using System.IO;

namespace Fred68.Tools.Engineering
	{
	/// <summary> Classe statica con alcune funzioni di calcolo ottico </summary>
	public static class Ottica 
		{
		#pragma warning disable 1591
		private static readonly double epsilon = System.Double.Epsilon;					// Epsilon
		public static double Epsilon { get { return epsilon; } }		
		#pragma warning restore 1591
		#region RIFLESSIONE E RIFRAZIONE
		// Restituisce versore (oppure vettore nullo se riflessione totale)
		/// <summary>
		/// Ottiene il versore riflesso
		/// </summary>
		/// <param name="versoreIn">Versore raggio entrante</param>
		/// <param name="versoreNorm">Versore normale uscente</param>
		/// <returns></returns>
		public static Point2D Riflesso(Point2D versoreIn, Point2D versoreNorm)			// Riflessione (versori, tutti modulo 1) 
			{
			Point2D r = versoreIn - 2 * (versoreIn^versoreNorm) * versoreNorm;
			return r;
			}
		/// <summary>
		/// Ottiene il versore rifratto
		/// </summary>
		/// <param name="versoreIn">Versore raggio entrante</param>
		/// <param name="versoreNorm">Versore normale uscente</param>
		/// <param name="n1">indice rifrazione mezzo attuale</param>
		/// <param name="n2">indice rifrazione prossimo mezzo</param>
		/// <returns></returns>
		public static Point2D Rifratto(Point2D versoreIn, Point2D versoreNorm, double n1, double n2)	// Rifrazione (versori) 
			{
			Point2D r = new Point2D();
			if( (n1>0) && (n2>0) )
				{
				double nn, ct, st2;
				nn = n1 / n2;				// Rapporto tra gli indici di rifrazione
				ct = versoreIn ^ versoreNorm;			// Coseno angolo incidente
				st2 = nn*nn*(1-ct*ct);		// Seno angolo riflesso, al quadrato
				if(st2 <= 1.0)				// Se entro angolo limite (non riflessione totale)
					{
					r = nn*versoreIn - versoreNorm*(nn*ct+Math.Sqrt(1-st2));
					}
				}
			return r;
			}
		#endregion
		#region LUNGHEZZE D'ONDA
		/// <summary>
		/// Lunghezze d'onda comuni
		/// in nm (1e-9)
		/// </summary>
		public static class LunghezzaOnda
			{
			#pragma warning disable 1591
			public static double linea_t = 1014.0;
			public static double linea_S = 852.1;
			public static double linea_A1 = 768.19;
			public static double linea_r = 706.5;
			public static double linea_C = 656.27;
			public static double linea_C1 = 643.8;
			public static double linea_HeNe = 632.8;
			public static double linea_D = 589.3;
			public static double linea_d = 587.56;
			public static double linea_e = 546.07;
			public static double linea_F = 486.1;
			public static double linea_F1 = 479.99;
			public static double linea_g = 435.83;
			public static double linea_h = 404.66;
			public static double linea_i = 365.01;
			public static double Rosso = 720.0;
			public static double Verde = 560.0;
			public static double Blu = 460.0;
			public static double InfrarossoCO2 = 10600.0;
			public static double InfrarossoFibra = 1064.0;
			#pragma warning restore 1591
			}
		#endregion
		#region MATERIALI OTTICI COMUNI
		/// <summary>
		/// Classe di appoggio per i materiali ottici comuni
		/// </summary>
		public static class MaterialiOttici
			{
			#warning Completare il costruttore MaterialiOttici() che carichi coefficienti di Laurent e nome da un file
			static List<MaterialeOttico> mt = new List<MaterialeOttico>();
			/// <summary>
			/// Costruttore statico.
			/// Legge il file Mat.dat nella stessa cartella dell'eseguibile
			/// </summary>
			static MaterialiOttici()
				{
				string nomefile = "Mat.dat";
				string linea;
				int i;
				StreamReader sr = new StreamReader(nomefile);
				TokenString tk = new TokenString();						// Tokenizzatore
				while(!sr.EndOfStream)
				    {
				    linea = sr.ReadLine();					// Legge una riga (ossia un oggetto completo)
				    tk.Set(ref linea, "\t ");				// Imposta il tokenizzatore, str per reference
					double[] A = new double[MaterialeOttico.NA];
					string nomemat="-";
					i = 0;									// Azzera contatore
					foreach (string s in tk)
						{
						switch (i)
							{
							case 0:
								nomemat = s;
								break;
							default:
								if((i>0)&&(i<MaterialeOttico.NA))
									{
									double val;
									if (double.TryParse(	s,
															System.Globalization.NumberStyles.Float,
															System.Globalization.CultureInfo.CreateSpecificCulture("en-US"),
															out val))
										A[i-1] = val;
									else
										throw new Exception("Fallita conversione valori materiali da file");
									}
								break;
							}
						i++;
						}	
					mt.Add(new MaterialeOttico(nomemat,A[0],A[1],A[2],A[3],A[4],A[5]));
					}
				}
			/// <summary>
			/// Estrae il materiale ottico in base al nome
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>
			public static MaterialeOttico Get(string name)
				{
				MaterialeOttico found = null;
				found = mt.Find(delegate(MaterialeOttico o) { if(o.Nome == name)
				                                                    return true;
				                                                return false; });
				return found;
				}
			/// <summary>
			/// Restituisce la lista dei materiali ottici disponibili
			/// </summary>
			/// <returns></returns>
			public static string Lista()
				{
				StringBuilder str = new StringBuilder();
				foreach(MaterialeOttico m in mt)
					str.Append(m.Nome+" / ");
				return str.ToString();
				}
			}
		#endregion
		}

	/// <summary> Classe base per oggetti ottici in genere </summary>
	public abstract class OggettoOttico : IPlot, ITextFile
		{
		/// <summary>Distanza minima due punti distinti</summary>
		protected static readonly double coincidencedistance = 1E-12;
		/// <summary>Distanza minima due punti distinti</summary>
		public static double CoincidenceDistance { get { return coincidencedistance; } }
		/// <summary>
		/// Nome standard iniziale
		/// </summary>
		public static readonly string nomestandard = "-";
		/// <summary>
		/// descrittore (usato se scrittura su stream)
		/// </summary>
		public static readonly string descrittore = "OGGETTO"; 
		/// <summary>
		/// lista caratteri validi per il nome
		/// </summary>
		public static readonly string caratteriValidi="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_";
		#region PROTECTED
		#pragma warning disable 1591
		protected string nome;
		protected bool bValid;						// Flag
		#pragma warning restore 1591
		#endregion
		#region PROPRIETA
		/// <summary>
		/// Nome dell'oggetto
		/// </summary>
		public string Nome
			{
			get {return nome;}
			set {nome = value;}
			}		
		#endregion
		#region COSTRUTTORI
		/// <summary>
		/// Costruttore
		/// </summary>
		public OggettoOttico()
			{
			nome = nomestandard;
			}
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="nameogg"></param>
		public OggettoOttico(string nameogg)
			{
			#warning FilterName da provare (probabilmente ok)
			nome = FilterName(nameogg);
			}
		#endregion
		/// <summary>
		/// true se valido
		/// </summary>
		public bool IsValid 
			{
			get { return bValid; }
			}
		/// <summary>
		/// Filtra solo i caratteri validi
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public string FilterName(string n)
			{
			StringBuilder str = new StringBuilder();
			foreach(char c in n)
				{
				if(caratteriValidi.Contains(c.ToString()))
					str.Append(c);
				}
			return str.ToString();
			}
		#pragma warning disable 1591
		public abstract void Validate();
		public abstract void Plot(Graphics dc, Finestra fin, Pen penna);
		public abstract void Display(DisplayList displaylist, int penna);
		public abstract Intersection TrovaIntersezione(Line2D lin);
		public abstract List<Raggio> CalcolaRaggi(Raggio rIncidente, MaterialeOttico ambiente);
		#warning DA ABILITARE ! Poi completare le altre classi
		// public abstract bool Scrivi(StreamWriter sw);
		// public abstract bool Leggi(StreamReader sr);

		#pragma warning restore 1591
		}

	/// <summary> Classe con i materiali </summary>
	public class MaterialeOttico		: OggettoOttico 
		{
		#warning Verificare la classe MaterialeOttico
		#pragma warning disable 1591
		public static readonly double n_vuoto = 1.0;
		public static readonly string nome_vuoto = "Vuoto";
		protected double[] A;		// I coefficienti
		protected double n_;		// indice costante
		bool costante;				// Se n unico
		public static int NA = 6;
		#pragma warning restore 1591
		/// <summary>
		/// Azzera e imposta n = 1
		/// Non modifica il nome
		/// </summary>
		public void Clear(double n = 1.0)
			{
			for(int i=0; i<NA; i++)
				A[i] = 0.0;
			A[0] = n*n;
			n_ = n;
			costante = true;
			}
		/// <summary>
		/// Costruttore vuoto
		/// </summary>
		public MaterialeOttico() : base(nome_vuoto) 
			{
			A = new double[NA];
			Clear();
			}
		/// <summary>
		/// Costruttore con indice di rifrazione costante
		/// </summary>
		/// <param name="Nome">Nome del materiale</param>
		/// <param name="Nrifrazione">Indice di rifrazione</param>
		public MaterialeOttico(string Nome, double Nrifrazione) : base(Nome) 
			{
			A = new double[NA];
			this.nRifrazione = Nrifrazione;
			Validate();
			}
		/// <summary>
		/// Costruttore con indice di rifrazione variabile
		/// Parametri della serie di Laurent
		/// </summary>
		/// <param name="Nome">Nome del materiale</param>
		/// <param name="A0"></param>
		/// <param name="A1"></param>
		/// <param name="A2"></param>
		/// <param name="A3"></param>
		/// <param name="A4"></param>
		/// <param name="A5"></param>
		public MaterialeOttico(string Nome, double A0, double A1, double A2, double A3, double A4, double A5) : base(Nome) 
			{
			A = new double[NA];
			this.SetnRifrazione(A0,A1,A2,A3,A4,A5);
			Validate();
			}
		/// <summary>
		/// Costruttore con indice di rifrazione variabile
		/// Parametri della serie di Laurent
		/// </summary>
		/// <param name="Nome">Nome del materiale</param>
		/// <param name="A">Array di double di 6 elementi</param>
		public MaterialeOttico(string Nome, double[] A) : base(Nome) 
			{
			A = new double[NA];
			this.SetnRifrazione(A);
			Validate();
			}

		/// <summary>
		/// Verifica e corregge
		///	Controlla solo A[0]
		/// </summary>
		public override void Validate()
			{
			bValid = false;
			if(A[0] > Double.Epsilon)
				bValid = true;
			}
		/// <summary>
		/// Indice di rifrazione (costante)
		/// </summary>
		public double nRifrazione 
			{
			get	{
				return GetnRifrazione();
				}
			set	{
				SetnRifrazione(value);
				}
			}
		/// <summary>
		/// Restituisce l'indice di rifrazione, se unico
		///	oppure NaN in tutti gli altri casi
		/// </summary>
		/// <returns></returns>
		public double GetnRifrazione()
			{
			if(!costante)
				return Double.NaN;
			return n_;
			}
		/// <summary>
		/// Restituisce l'indice di rifrazione ad una lunghezza d'onda,
		/// pari al valore costante oppure calcolato con la serie di Laurent
		/// Al momento non e' utilizzata la serie di Sellmeier
		/// </summary>
		/// <param name="lambda">Lunghezza d'onda in nm (1e-9 m)</param>
		/// <returns></returns>
		public double GetnRifrazione(double lambda)
			{
			#warning GetnRifrazione() da controllare
			if(costante)
				return n_;
			double n2, lmicron;
			lmicron = lambda / 1000;
			n2 =	A[0] +
					A[1]*Math.Pow(lmicron,2.0) +
					A[2]*Math.Pow(lmicron,-2.0) +
					A[3]*Math.Pow(lmicron,-4.0) +
					A[4]*Math.Pow(lmicron,-6.0) +
					A[5]*Math.Pow(lmicron,-8.0);
			if(n2 < Double.Epsilon)
				return Double.NaN;
			return Math.Sqrt(n2);
			}
		/// <summary>
		/// Imposta l'indice di rifrazione (costante)
		/// </summary>
		/// <param name="n"></param>
		public void SetnRifrazione(double n)
			{
			Clear(n);
			Validate();
			}
		/// <summary>
		/// Imposta i coefficienti della serie di Laurent
		/// </summary>
		/// <param name="A0"></param>
		/// <param name="A1"></param>
		/// <param name="A2"></param>
		/// <param name="A3"></param>
		/// <param name="A4"></param>
		/// <param name="A5"></param>
		public void SetnRifrazione(double A0, double A1, double A2, double A3, double A4, double A5)
			{
			A[0] = A0;
			A[1] = A1;
			A[2] = A2;
			A[3] = A3;
			A[4] = A4;
			A[5] = A5;
			costante = false;
			}
		/// <summary>
		/// Imposta i coefficienti della serie di Laurent
		/// </summary>
		/// <param name="A">array double[6]: A0...A5</param>
		public void SetnRifrazione(double[] A)
			{
			if(A.Length != NA)
				Clear();
			else
				{
				for(int i=0; i<NA; i++)
					this.A[i] = A[i];
				}
			costante = false;
			Validate();
			}

		/// <summary>
		/// Plot (non fa nulla)
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="fin"></param>
		/// <param name="penna"></param>
		public override void Plot(Graphics dc, Finestra fin, Pen penna) 
			{}				// Non fa nulla
		/// <summary>
		/// Aggiunge alla display list (non fa nulla)
		/// </summary>
		/// <param name="displaylist"></param>
		/// <param name="penna"></param>
		public override void Display(DisplayList displaylist, int penna) 
			{}			// Non fa nulla
		/// <summary>
		/// Restituisce prima intersezione positiva con una linea (sempre null)
		/// </summary>
		/// <param name="lin"></param>
		/// <returns></returns>
		public override Intersection TrovaIntersezione(Line2D lin)
			{
			return null;
			}
		/// <summary>
		/// Restituisce la lista di raggi (sempre nulla, non interagisce)
		/// </summary>
		/// <param name="rIncidente"></param>
		/// <param name="ambiente"></param>
		/// <returns></returns>
		public override List<Raggio> CalcolaRaggi(Raggio rIncidente, MaterialeOttico ambiente)
			{
			return new List<Raggio>();
			}
		}

	/// <summary> Corpo ottico </summary>
	public class CorpoOttico			: OggettoOttico 
		{
		#region PROTECTED
		#pragma warning disable 1591
		protected static int nTrattiMin = 3;		// Numero minimo di tratti
		protected LinkedList<Contorno>	contorni;	// Lista dei tratti di contorno (in origine era List<Contorno>)
		protected MaterialeOttico materiale;		// Materiale con indice di rifrazione
		protected double dimcar;					// Dimensione caratteristica
		delegate bool Check();						// Delegate per eseguire i controlli di validita`
		delegate int ConfrontaIntersezioni(Intersection x, Intersection y);	// Delegate per eseguire ordinamento
		#pragma warning restore 1591
		#endregion
		#region PROPRIETA
		/// <summary>
		/// Indice di rifrazione
		/// </summary>
		public double nRifrazione 
		    {
		    get {return Materiale.nRifrazione;}
		    }
		/// <summary>
		/// Materiale ottico
		/// </summary>
		public MaterialeOttico Materiale
			{
			get { return materiale; }
			set	{ materiale = value; }
		    }
		/// <summary>
		/// Dimensione caratteristica
		/// </summary>
		public double DimCaratteristica 
			{
			get { return dimcar; }
			}
		/// <summary>
		/// Fattore moltiplicativo della dimensione caratteristica
		/// per calcolare un punto di poco fuori dalla suerficie
		/// </summary>
		public static double FrazioneEpsilon = 1e-4;	// Usata per il calcolo di un punto di poco fuori dalla superficie
		#endregion
		#region COSTRUTTORI
			/// <summary>
			/// Costruttore
			/// </summary>
		public CorpoOttico() 
			{
			contorni = new LinkedList<Contorno>();
			Materiale = new MaterialeOttico();
			bValid = false;
			}
		/// <summary>
		/// costruttore
		/// </summary>
		/// <param name="Nome"></param>
		public CorpoOttico(string Nome) : this()
			{
			this.Nome = Nome;
			}
			/// <summary>
			/// Costruttore
			/// </summary>
			/// <param name="mat"></param>
		public CorpoOttico(MaterialeOttico mat) : this() 
			{
			this.Materiale = mat;
			}
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="Nome"></param>
		public CorpoOttico(MaterialeOttico mat, string Nome) : this()
			{
			this.Materiale = mat;
			this.Nome = Nome;
			}
		#endregion
		#region FUNZIONI
			/// <summary>
			/// Verifica e corregge dove possibile
			/// </summary>
			/// <returns></returns>
		public override void Validate() 
			{
			Check[] ops;									// Array di delegate
			bValid = true;
			CalcolaDimCar();
			ops = new Check[] {CheckRifrazione, CheckNumTratti, CheckValidi, CheckConnesso, CheckDimCar};
				// Mancano controlli di:
				// punto interno
				// contorno che interseca se stesso
			foreach( Check op in ops)						// Chiama la funzione
				{
				if(!op())									// Se errore...
					{
					bValid = false;							// ...azzera il flag
					break;
					}
				}
			}
			/// <summary>
			/// Aggiunge un tratto di contorno, controlla se connesso.
			/// </summary>
			/// <param name="contorno"></param>
			/// <returns></returns>
		public bool Add(Contorno contorno) 
			{
			bool ok = false;
			if(contorno != null)
				{
				contorni.AddLast(contorno);							// Aggiunge
				ok = true;
				LinkedListNode<Contorno> nd = (contorni.Last).Previous;
				Contorno prev = (nd!=null) ? nd.Value : null;		// Legge il penultimo
				if(prev != null)									// Se c'e` (almeno 2 elementi)
					{												// Controlla se connesso.
					if(!Function2D.AreConnected(prev.Tratto,contorno.Tratto))	// Se no, errore
						ok = false;
					}
				}
			Validate();												// Esegue comunque il controllo di validita`
			return ok;
			}
			/// <summary>
			/// Azzera la lista dei contorni
			/// </summary>
		public void ClearContorni() 
			{
			this.contorni.Clear();
			Validate();
			}
			/// <summary>
			/// Trova intersezioni con la linea, solo se dopo P1, e le ordina
			/// </summary>
			/// <param name="lin">Linea</param>
			/// <returns></returns>
		public List<Intersection> TrovaIntersezioniPositive(Line2D lin) 
			{
			List<Intersection> li = new List<Intersection>();				// Nuova lista vuota
			foreach(Contorno c in contorni)									// Percorre i tratti di contorno
				{
				List<Intersection> lc;									
				lc = Function2D.Intersect(lin, c.Tratto, false, true);		// Cerca le intersezioni
				lc.ForEach(delegate(Intersection x)							// Le aggiunge solo se strettamente positive (oltre P1 verso P2)
									{										// Esclude la prima, se il punto iniziale della linea
									if( x.t1 > CoincidenceDistance)			// e` gia` sul contorno (era Double.Epsilon)
										{
										li.Add(x);
										}
									} );
				}
			li.Sort(CfrT1);													// Ordina per distanza crescente					
			li = EliminaIntersezioniDoppie(li);								// Elimina punti coincidenti
			li.Sort(CfrT1);													// Ordina di nuovo				
			return li;
			}
			/// <summary>
			/// Trova la prima intersezione della linea con il corspo ottico, da P1 in poi, oppure null.
			/// </summary>
			/// <param name="lin">La linea</param>
			/// <returns></returns>
		public override Intersection TrovaIntersezione(Line2D lin) 
			{
			List<Intersection> li = this.TrovaIntersezioniPositive(lin);
			if(li.Count > 0)
				return li[0];
			else
				return null; //new Intersection(null, 0, 0, null, null);
			}
			/// <summary>
		/// Funzione principale di calcolo ottico
		/// </summary>
		/// <param name="rIncidente"></param>
		/// <param name="ambiente"></param>
		/// <returns></returns>
		public override List<Raggio> CalcolaRaggi(Raggio rIncidente, MaterialeOttico ambiente) 
			{
			List<Raggio> lR = new List<Raggio>();			// Lista di raggi
			if(ambiente !=null)								// Controllo iniziale
				{
				Raggio r1 = null;							// Raggio entrante
				Raggio r2 = null;							// Raggio uscente
				for(r1 = rIncidente; r1 != null; r1 = r2)	// Ciclo di calcolo
					{
					r2 = null;
					Intersection fint = TrovaIntersezione(r1);		// Trova prima intersezione di r1 con il corpo ottico
					if(fint != null)								// Se non la trova, r2 resta null	
						{
						List<Contorno> lc = Belongs(fint.p);		// Trova i contorni cui appartiene la prima intersezione
						if(lc.Count > 2)							// Se piu` di due tratti: errore
							throw new Exception("Intersezione unica di un raggio con piu` di due tratti, in CalcolaRaggio()");
						if(lc.Count == 2)								// Se due tratti: su vertice
					        {
					        if(Tangenti(lc[0].Tratto, lc[1].Tratto))	// Se tangenti, considero intersezione su uno dei due, equivalente
					            {
					            if(lc[0].Stato != lc[1].Stato)			// Se hanno uno stato superficiale diverso, elimina il raggio
					                {
					                lc.Clear();
					                }
					            else									// altrimenti
					                lc.Remove(lc[1]);					// elimino l'ultimo contorno...
					            }										// ...e proseguo al prossimo if
					        }
						if(lc.Count == 1)								// Se una sola intersezione
							{
							Point2D versoreIn = r1.Vector();			// Versori entrante (Raggio gia` normalizzato) e normale.
							Point2D versoreNorm = Function2D.VersorOut(fint.p, lc[0].Tratto, r1.Point(fint.t1 - dimcar*FrazioneEpsilon));
							switch(lc[0].Stato)
								{
								case StatoSuperficie.Opaca:
					                {
					                break;								// Non fa nulla, raggio assorbito, nessun raggio in uscita
					                }
								case StatoSuperficie.Riflettente:		// Calcola raggio riflesso
					                {									
					                r2 = new Raggio(new Line2D(fint.p, Ottica.Riflesso(versoreIn, versoreNorm), true),r1.Lambda);
					                r2.CorpoAttuale = r1.CorpoAttuale;
					                break;
					                }
					            case StatoSuperficie.Trasparente:
					                {
					                MaterialeOttico co1, co2;			// Corpi ottici del raggio in ingresso ed uscita
					                co1 = r1.CorpoAttuale;
					                if(co1 == this.Materiale)			// Se il raggio entrante si trova nel corpo ottico
					                    {
					                    co2 = ambiente;					// quello uscente nell'ambiente
					                    }
					                else								// se no, da ambiente...
					                    {
					                    co2 = this.Materiale;			// ...a corpo attuale
					                    }
					                Point2D vrifr = Ottica.Rifratto(versoreIn, versoreNorm, co1.nRifrazione,  co2.nRifrazione);
					                if(vrifr != null)
					                    {
					                    r2 = new Raggio(new Line2D(fint.p, vrifr, true),r1.Lambda);
										r2.CorpoAttuale = co2;
					                    #warning Manca determinazione se il raggio parte dall'esterno o dall'interno
					                    }
					                break;
					                }
								}
							
							}
						r1.T2r = fint.t1;
						}
					lR.Add(r1);
					}
				}
			return lR;
			}
			/// <summary>
		/// Trova il contorno costituito dal tratto 
		/// </summary>
		/// <param name="tr">Tratto</param>
		/// <returns></returns>
		public Contorno Contorno(Tratto tr) 
			{
			Contorno cf = null;
			foreach(Contorno c in contorni)
				{
				if(c.Tratto == tr)
					{
					cf = c;
					break;
					}
				}
			return cf;
			}
			/// <summary>
		/// Plot
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="fin"></param>
		/// <param name="penna"></param>
		public override void Plot(Graphics dc, Finestra fin, Pen penna)
			{
			foreach(Contorno cn in contorni)
				{
				cn.Plot(dc,fin,penna);
				}
			}
			/// <summary>
		/// Aggiunge alla display list
		/// </summary>
		/// <param name="displaylist"></param>
		/// <param name="penna"></param>
		public override void Display(DisplayList displaylist, int penna)
			{
			foreach(Contorno cn in contorni)
				{
				cn.Display(displaylist,penna);
				}
			}
		#region PROTECTED FUNC
			/// <summary>
		/// Controlla che vi siano almeno tre tratti di contorno
		/// </summary>
		/// <returns></returns>
		protected bool CheckNumTratti() 
			{
			#warning Verificare: valido se solo un Circle
			bool ret = true;
			if(contorni.Count < CorpoOttico.nTrattiMin)		// Controlla contorni.Count
				{
				ret = false;
				if(contorni.Count==1)
					{
					if(contorni.First.Value.Tratto.GetType()==typeof(Arc2D))
						{
						if(((Arc2D)contorni.First.Value.Tratto).IsCircle)
							ret = true;
						}
					}
				}
			return ret;
			}
			/// <summary>
		/// Controlla se connesso
		/// </summary>
		/// <returns></returns>
		protected bool CheckConnesso() 
			{
			#warning Correggere: se solo un Circle: valido
			bool connesso = true;										// Imposta flag come se tutto connesso.
			if(contorni.Count<2)
				return false;											// Se solo un elemento, esce subito
			Contorno c1, c2;
			LinkedListNode<Contorno> n1, n2;
			for(n1 = contorni.First; (n2 = n1.Next) != null; n1 = n2)	// Percorre la lista doppio linkata
				{
				c1 = n1.Value;											// Legge un contorno ed il successivo
				c2 = n2.Value;
				if(!Function2D.AreConnected(c1.Tratto,c2.Tratto))		// Se non connessi, imposta false e uscita alla prossima iterazione
					{
					connesso = false;
					n1 = contorni.Last;
					}
				}
			if(connesso)												// Ultimo controllo, tra l'ultimo ed il primo
				{
				c1 = contorni.Last.Value;
				c2 = contorni.First.Value; 
				if( !Function2D.AreConnected(c1.Tratto,c2.Tratto))		// Se non connessi, azzera il flag
					connesso = false;
				}
			return connesso;
			}
			/// <summary>
		/// Controlla se tratti balidi
		/// </summary>
		/// <returns></returns>
		protected bool CheckValidi() 
			{
			bool ok = true;
			foreach(Contorno c in contorni)
				{			
				//return !contorni.Exists(r => r.Tratto.IsValid == false);
				if(! c.Tratto.IsValid)
					ok = false;
				}
			return ok;
			}
			/// <summary>
		/// Controlla rifrazione
		/// </summary>
		/// <returns></returns>
		protected bool CheckRifrazione() 
			{
			if(Materiale.nRifrazione <= Double.Epsilon)					// Controlla n e corregge
				return false;
			return true;
			}
			/// <summary>
		/// Controlla che dimensione caratteristica non nulla
		/// </summary>
		/// <returns></returns>
		protected bool CheckDimCar() 
			{
			if(dimcar <= Double.Epsilon)
				{
				return false;
				}
			return true;
			}
			/// <summary>
			/// Elimina intersezioni doppie e restituisce nuova lista
			/// </summary>
			/// <param name="li">Lista ORDINATA in base a t1</param>
			/// <returns></returns>
		protected List<Intersection> EliminaIntersezioniDoppie(List<Intersection> li) 
			{
			List<Intersection> lp;												// Nuova lista
			LinkedList<Intersection> ll = new LinkedList<Intersection>(li);		// Nuova lista connessa, copiata dalla precedente
			if(ll.Count >= 2)
				{
				// Vari casi possibili di intersezioni multiple (intersezione coincidente singola o doppia, una o entrambe tangenti).
				// Complesso. Si eliminano le intersezioni doppie SOLO in base al paramentro della retta.
				LinkedListNode<Intersection> i1,i2;							
				for(i1=ll.First; (i2 = i1.Next) != null; i1 = i2)	// Percorre la lista, ottiene elemento e successivo
					{
					if( Math.Abs(i1.Value.t1 - i2.Value.t1) <= Function2D.CoincidenceDistance)	// Se i2 coincide con i1...
						{
						ll.Remove(i2);								// Elimina i2
						i2 = i1;									// Imposta in modo da restare su i1 alla prossima iterazione
						}
					}
				}	
			lp = new List<Intersection>(ll);						// Crea nuova lista copiata dalla lista connessa depurata
			return lp;
			}
			/// <summary>
			/// Delegate di confronto
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
		protected int CfrT1(Intersection x, Intersection y) 
			{
			if(Math.Abs(x.t1 - y.t1) < Line2D.CoincidenceDistance)
				return 0;
			if(x.t1 < y.t1)
				return -1;
			else
				return 1;
			}
			/// <summary>
			/// Calcola dimensione caratteristica
			/// </summary>
		protected void CalcolaDimCar() 
			{
			double xmin, xmax, ymin, ymax, dx, dy;
			xmin = ymin = Double.MaxValue;
			xmax = ymax = Double.MinValue;
			if(contorni.Count > 0)						// Se ha almeno un contorno
				{
				foreach(Contorno c in contorni)
					{
					Point2D[] p;
					p = c.Tratto.P12;
					foreach(Point2D pt in p)
						{
						if(xmin >= pt.x)	xmin = pt.x;
						if(ymin >= pt.y)	ymin = pt.y;
						if(xmax <= pt.x)	xmax = pt.x;
						if(ymax <= pt.y)	ymax = pt.y;
						}
					}	
				dx = xmax - xmin;
				dy = ymax - ymin;
				dimcar = Math.Sqrt(Math.Abs(dx * dy));
				}
			}
			/// <summary>
			/// Restituisce la lista dei contorni del corpo cui appartiene il punto
			/// </summary>
			/// <param name="pt">Il punto</param>
			/// <returns></returns>
		protected List<Contorno> Belongs(Point2D pt) 
			{
			List<Contorno> lc = new List<Contorno>();		// Crea lista vuota
			foreach(Contorno c in contorni)					// Percorre i contorni
				{
				if(c.Tratto.Belongs(pt, true))				// Se il punto appartiene, aggiunge alla lista
					{
					lc.Add(c);
					}
				}
			return lc;
			}
			/// <summary>
			/// Verifica se pVertice e` un vertice del corpo ottico
			/// </summary>
			/// <param name="pVertice">Il punto</param>
			/// <returns>Intersezione fittizia, contenente il rif. ai due tratti di contorno a cui appartiene</returns>
		protected Intersection CheckVertice(Point2D pVertice) 
			{
			Intersection i = null;
			List<Contorno> lc;
			lc = Belongs(pVertice);							// Ottiene la lista
			if(lc.Count == 2)								// Se due Tratti: e` un vertice; se meno, no
				{
				i = new Intersection(pVertice, 0.0, 0.0, lc[0].Tratto, lc[1].Tratto);
				}
			if(lc.Count > 2)
				throw new Exception("Profilo con punto comune a piu` di due tratti");
			return i;
			}
			/// <summary>
			/// Verifica se i due tratti sono tangenti all'estremo
			/// </summary>
			/// <param name="t1">Tratto 1</param>
			/// <param name="t2">Tratto 2</param>
			/// <returns></returns>
		protected bool Tangenti(Tratto t1, Tratto t2) 
			{
			bool tg = false;
			bool conn = false;
			int i,j, itrov, jtrov;
			itrov = jtrov = -1;
			for(i=0; i<2; i++)
				for(j=0; j<2; j++)
					{
					if ( Function2D.Distance(t1.P12[i],t2.P12[j]) <= Function2D.CoincidenceDistance)
						{
						conn = true;			// Imposta indici trovati, flag... 
						itrov = i;
						jtrov = j;
						i=j=2;					// ...e uscita dai cicli
						}
					}
			if(conn)							// Se connessi ai vertici indicati...
				{
				Point2D norm1 = Function2D.VersorOut(t1.P12[itrov],t1);		// Trova le normali
				Point2D norm2 = Function2D.VersorOut(t2.P12[jtrov],t2);
				if( Math.Abs(norm1^norm2) >= 1 - 2 * Function2D.CoincidenceDistance )
					{
					tg = true;
					}
				}
			return tg;
			}
		#endregion
		#endregion
		}	

	/// <summary>Sorgente ottica </summary>
	public class SorgenteOttica : OggettoOttico 
		{
		#warning SorgenteOttica da completare !
		/// <summary> Lunghezza d'onda </summary>
		double lambda;
		/// <summary> Tipi di sorgente </summary>
		public enum	TipoSorgente
						{
						/// <summary> sorgente puntiforme (non solo omnidirezionale) </summary>
						puntiforme = 0,
						/// <summary> sorgente collimata (con divergenza) </summary>
						collimata = 1
						};
		/// <summary> Tipo di sorgente </summary>
		TipoSorgente tipoSorgente;
		/// <summary> Arco (se sorgente puntiforme) </summary>
		Arc2D arco;
		/// <summary>Origine e versore (se sorgente collimata)</summary>
		Line2D lineaRaggio;
		/// <summary>Diametro (se sorgente collimata)</summary>
		double diametro;
		/// <summary>Divergenza (se sorgente collimata). Angolo raggio laterale rispetto all'asse, in rad.</summary>
		Angolo divergenza;
		/// <summary>Numero di raggi da generare </summary>
		int nraggi;
		#region PROPRIETA
		/// <summary>
		/// Tipo di sorgente
		/// </summary>
		public TipoSorgente Sorgente
			{
			get {return tipoSorgente;}
			}
		/// <summary>
		/// Punto di origine
		/// </summary>
		public Point2D Origine
			{
			get {return lineaRaggio.P1;}
			set {lineaRaggio.P1 = value; Validate();}
			}
		/// <summary>
		/// Versore di direzione
		/// </summary>
		public Point2D Direzione
			{
			get {return lineaRaggio.Vector();}
			set
				{
				lineaRaggio.P2 = lineaRaggio.P1 + value;
				Validate();
				}
			}
		/// <summary>
		/// Diametro del fascio
		/// </summary>
		public double Diametro
			{
			get {return diametro;}
			set {diametro = value; Validate();}
			}
		/// <summary>
		/// Divergenza in radianti
		/// </summary>
		public double Divergenza
			{
			get {return divergenza;}
			set {divergenza = value; Validate();}
			}
		/// <summary>
		/// Lunghezza d'onda in nm (1e-9)
		/// </summary>
		public double Lambda
			{
			get {return lambda;}
			set {lambda = value; Validate();}
			}
		/// <summary>
		/// Numero di raggi emessi
		/// </summary>
		public int nRaggi
			{
			get {return nraggi;}
			set {nraggi = value; Validate();}
			}
		#endregion
		#region COSTRUTTORI
		/// <summary>Costruttore vuoto</summary>
		public SorgenteOttica() : base()
			{
			tipoSorgente = TipoSorgente.collimata;
			lineaRaggio = new Line2D();
			divergenza = new Angolo(0.0);
			Validate();
			}
		/// <summary>Costruttore di copia</summary>
		/// <param name="sorgente"></param>
		public SorgenteOttica(SorgenteOttica sorgente) : base(sorgente.Nome)	
			{
			this.tipoSorgente = sorgente.tipoSorgente;
			this.arco = sorgente.arco;
			this.diametro = sorgente.diametro;
			this.divergenza = sorgente.divergenza;
			this.lambda = sorgente.lambda;
			this.nraggi = sorgente.nraggi;
			this.lineaRaggio = sorgente.lineaRaggio;
			Validate();
			}
		/// <summary>
		/// Costruttore per sorgente collimata
		/// </summary>
		/// <param name="nome">Nome oggetto</param>
		/// <param name="origine">Punto di origine</param>
		/// <param name="angolo">Angolo dell'asse del raggio</param>
		/// <param name="radianti">true se in radianti</param>
		/// <param name="diametro">Diametro del fascio</param>
		/// <param name="divergenza">Divergenza in radianti (tra raggio estremo ed asse)</param>
		/// <param name="lambda">Lunghezza d'onda</param>
		/// <param name="nraggi">Numero di raggi </param>
		public SorgenteOttica(string nome, Point2D origine, double angolo, bool radianti, double diametro, double divergenza, double lambda, int nraggi) : base(nome)
			{
			tipoSorgente = TipoSorgente.collimata;
			lineaRaggio = new Line2D(origine.x,origine.y,angolo,1.0,true,true);
			this.diametro = diametro;
			this.divergenza = divergenza;
			this.lambda = lambda;
			this.nraggi = nraggi;
			Validate();
			}
		#endregion
		/// <summary>
		/// Crea la lista dei raggi generati dalla sorgente ottica
		/// </summary>
		/// <returns></returns>
		public List<Raggio> CreaRaggi()
			{
			List<Raggio> lr = new List<Raggio>();
			int nr = (int)(((float)nraggi)/2.0 + float.Epsilon) + 1;
			Point2D vettore = lineaRaggio.Vector();					// Direzione
			double alfa = vettore.Alfa();							// Angolo
			Point2D normale = vettore.Normal();						// Normale
			Point2D vmeta = normale * diametro * 0.5;				// Vettore di meta` lunghezza 
			for(int i=0; i<nr; i++)
				{
				Point2D vp, vm, pp, pm;
				double ap, am, ang;
				double fraz = ((double)i)/((double)nr);				// Frazione punti i-esimo
				vp = fraz * vmeta;									// Offset dei due raggi
				vm = - vp;
				pp = lineaRaggio.P1 + vp;							// Origini
				pm = lineaRaggio.P1 + vm;
				ang = divergenza * fraz;							// Angoli dei due raggi
				ap = alfa + ang;
				am = alfa - ang;
				Line2D lp,lm;
				lp = new Line2D(pp.x,pp.y,ap,1.0,true,true);		// Linee
				lm = new Line2D(pm.x,pm.y,am,1.0,true,true);
				Raggio rp, rm;
				rp = new Raggio(lp,this.lambda);					// Raggi
				rm = new Raggio(lm,this.lambda);
				lr.Add(rp);											// Li aggiunge alla lista
				lr.Add(rm);
				}
			return lr;
			}
		/// <summary>
		/// Validate (da classe base astratta)
		/// </summary>
		public override void Validate()
			{
			bValid = false;												// Imposta come non valido, se verifica fallisce: esce
			if(lambda <= double.Epsilon) return;						// Lunghezza d'onda
			if(nraggi<1)	return;										// Numero di raggi
			switch(tipoSorgente)
				{
				case TipoSorgente.collimata:
					{
					if(lineaRaggio.Length <= Line2D.CoincidenceDistance)		return;		// Raggio non nullo
					if(diametro < 0.0)		return;									// Diametro nullo o positivo
					if(( divergenza <= -Math.PI/4.0)||(divergenza >= Math.PI/4.0))	// Divergenza
						return;
					}
					break;
				case TipoSorgente.puntiforme:
					{
					#warning Validate puntiforme da completare
					return;
					}
				default:
					return;
				}
			bValid = true;				// Imppsta come valido solo se superati tutti i test
			}
		/// <summary>
		/// Da completare
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="fin"></param>
		/// <param name="penna"></param>
		public override void Plot(Graphics dc, Finestra fin, Pen penna)
			{
			#warning SorgenteOttica.Plot da provare !
			if(IsValid)
				{
				Point start, end;

				Point2D vettore = lineaRaggio.Vector();					// Direzione
				//double alfa = vettore.Alfa();							// Angolo
				Point2D normale = vettore.Normal();						// Normale
				Point2D vmeta = normale * diametro * 0.5;				// Vettore di meta` lunghezza 

				Point2D pp, pm;
				pp = lineaRaggio.P1 + vmeta;							// Origini
				pm = lineaRaggio.P1 - vmeta;

				start = fin.Get(pp);
				end = fin.Get(pm);
				dc.DrawLine(penna,start,end);
				
				dc.DrawLine(penna,start,end);
				}
			}
		/// <summary>
		/// Da compleatare
		/// </summary>
		/// <param name="displaylist"></param>
		/// <param name="penna"></param>
		public override void Display(DisplayList displaylist, int penna)
			{
			#warning SorgenteOttica.Display ATTENZIONE !!!! Probabile oggetto derivato da Tratto !!!!
			}
		/// <summary>
		/// Restituisce prima intersezione positiva con una linea (sempre null)
		/// </summary>
		/// <param name="lin"></param>
		/// <returns></returns>
		public override Intersection TrovaIntersezione(Line2D lin)
			{
			return null;
			}
		/// <summary>
		/// Restituisce la lista di raggi (sempre nulla, non interagisce)
		/// </summary>
		/// <param name="rIncidente"></param>
		/// <param name="ambiente"></param>
		/// <returns></returns>
		public override List<Raggio> CalcolaRaggi(Raggio rIncidente, MaterialeOttico ambiente)
			{
			return new List<Raggio>();
			}
		}

	/// <summary> Ambiente </summary>
	public class Ambiente : IPlot
		{
		#pragma warning disable 1591
		protected MaterialeOttico matrice;
		protected List<OggettoOttico> oggetti;
		#pragma warning restore 1591
		/// <summary>
		/// Costruttore
		/// </summary>
		public Ambiente() 
			{
			matrice = new MaterialeOttico("vuoto", 1.0);
			oggetti = new List<OggettoOttico>();
			}
		/// <summary>
		/// Svuota
		/// </summary>
		public void CancellaOggetti() 
			{
			oggetti.Clear();
			}
		/// <summary>
		/// Materiale ottico della matrice
		/// </summary>
		public MaterialeOttico Matrice 
			{
			get {return matrice;}
			set {matrice = value;}
			}
		/// <summary>
		/// Numero di oggetti
		/// </summary>
		public int NumeroOggetti 
			{
			get{ return oggetti.Count;}
			}
		/// <summary>
		/// Aggiunge un oggetto
		/// se e` valido e se il nome non esiste ancora
		/// </summary>
		/// <param name="ogg"></param>
		/// <returns></returns>
		public bool Add(OggettoOttico ogg)
			{
			bool ret = false;
			if(ogg.IsValid)
				{
				if(!ExistsOggetto(ogg.Nome))
						{
						oggetti.Add(ogg);
						ret = true;
						}
				}
			return ret;
			}
		/// <summary>
		/// Restuisce una stringa con i nomi degli oggetti contenuti
		/// </summary>
		/// <returns></returns>
		public string ListaNomi() 
			{
			StringBuilder str = new StringBuilder();
			foreach(OggettoOttico ogg in oggetti)
				{
				str.Append(ogg.Nome + "\n");
				}
			return str.ToString();
			}
		/// <summary>
		/// Ottiene il riferimento ad un oggetto in base al nome
		/// </summary>
		/// <param name="nome"></param>
		/// <returns></returns>
		public OggettoOttico GetOggetto(string nome) 
			{
			OggettoOttico found = null;
			found = oggetti.Find(delegate(OggettoOttico o) { if(o.Nome == nome)
				                                                    return true;
				                                                return false; });
			return found;
			}
		/// <summary>
		/// Verifica se esiste un oggetto con quel nome
		/// </summary>
		/// <param name="nome"></param>
		/// <returns></returns>
		public bool ExistsOggetto(string nome) 
			{
			if(oggetti.Exists(delegate(OggettoOttico o) { if(o.Nome == nome)
				                                                    return true;
				                                                return false; }))
				{
				return true;
				}
			return false;
			}
		/// <summary>
		/// Plot
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="fin"></param>
		/// <param name="penna"></param>
		public void Plot(Graphics dc, Finestra fin, Pen penna) 
			{
			foreach(OggettoOttico ogg in oggetti)
				{
				ogg.Plot(dc,fin,penna);
				}
			}
		/// <summary>
		/// Aggiunge alla display list
		/// </summary>
		/// <param name="displaylist"></param>
		/// <param name="penna"></param>
		public void Display(DisplayList displaylist, int penna) 
			{
			foreach(OggettoOttico ogg in oggetti)
				{
				ogg.Display(displaylist,penna);
				}
			}
		/// <summary>
		/// Calcola il percorso di un raggio
		/// </summary>
		/// <param name="rIncidente"></param>
		/// <returns></returns>
		public List<Raggio> CalcolaRaggio(Raggio rIncidente)
			{
			Raggio rIni;
			List<Raggio> lr = new List<Raggio>();		// Crea la lista complessiva, vuota
			if(rIncidente == null)						// Se raggioIni e` nullo, esce dalla funzione
				return lr;
			if(!rIncidente.IsValid)
				return lr;
			rIni = rIncidente;							// Imposta raggioIni pari a rIncidente.
			for(int countmax = 1000; (rIni != null) && (countmax>0); countmax--)	// Ciclo di calcolo
				{			
				OggettoOttico primoOgg = null;				// Inizializza il riferimento 
				double tmin = double.MaxValue;				// e il parametro tmin
				Intersection inter;
				foreach(OggettoOttico ogg in oggetti)		// Trova il primo oggetto intersecato dal raggioIni
					{										// Percorre tutti gli elementi della lista ambient.
					inter = ogg.TrovaIntersezione(rIni);	// Trova la prima intersezione positiva
					if(inter != null)
						{
						if(inter.t1 < tmin)					// Se la t1 (relativa alla retta) e` minore di tmin
							{
							tmin = inter.t1;				// Memorizza t1 e oggetto
							primoOgg = ogg;
							}
						}
					}
				if(primoOgg != null)						// Se ha trovato il primo oggetto intersecato, prosegue.
					{										// Trova i raggi del primo oggetto
					List<Raggio> lro = primoOgg.CalcolaRaggi(rIni,this.matrice);
					if(lro.Count > 0)						// Se la lista non e` vuota
						{
						rIni = lro[lro.Count-1];			// Estrae l'ultimo elemento e lo mette nel raggioIni
						lro.RemoveAt(lro.Count-1);			// Elimina l'ultimo elemento della lista
						lr.AddRange(lro);					// Aggiunge la lista alla lista complessiva
						}
					}
				else										// Se non ha trovato il primo oggetto intersecato
					{
					lr.Add(rIni);							// Aggiunge rIni alla lista complessiva
					rIni = null;							// poi lo azzera;
					}
				}
			return lr;
			#warning Funzione principale di calcolo ottico da verificare !
			}
		/// <summary>
		/// Calcola i raggi generati dalla sorgente
		/// </summary>
		/// <returns></returns>
		public List<Raggio> CalcolaRaggi()
			{
			List<Raggio> lr = new List<Raggio>();
			List<Raggio> rs = new List<Raggio>();
			foreach(OggettoOttico ogg in oggetti)
				{
				if(ogg.GetType()==typeof(SorgenteOttica))
					{
					System.Windows.Forms.MessageBox.Show("Trovata sorgente "+ogg.Nome);
					rs.AddRange( ((SorgenteOttica)ogg).CreaRaggi() ); 
					}
				}
			foreach(Raggio r in rs)
				{
				r.CorpoAttuale = this.Matrice;
				lr.AddRange(CalcolaRaggio(r));
				}
			return lr;
			}
		}

	}

