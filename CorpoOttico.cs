using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fred68.Tools.Utilita;
using Fred68.Tools.Matematica;

#error "Non utilizzare il file CorpoOttico.cs ! La definizione della classe e` stata spostata in Ottica.cs"
// Test Github


namespace Fred68.Tools.Engineering
	{
	/// <summary>
	/// Corpo ottico
	/// </summary>
	class CorpoOttico : IValid
		{
		#region PROTECTED
		protected static int nTrattiMin = 3;		// Numero minimo di tratti
		protected LinkedList<Contorno>	contorni;	// Lista dei tratti di contorno (in origine era List<Contorno>)
		protected double n;							// Indice di rifrazione
		protected Materiale materiale;				// Materiale con indice di rifrazione
		protected double dimcar;					// Dimensione caratteristica
		protected bool bValid;						// Flag
		protected bool bAmbiente;					// E` un corpo infinito (ambiente)
		delegate bool Check();						// Delegate per eseguire i controlli di validita`
		delegate int ConfrontaIntersezioni(Intersection x, Intersection y);	// Delegate per eseguire ordinamento
		#endregion
		#region PROPRIETA
		public double IndiceRifrazione 
		    {
		    get {return n;}
			set	{
				n = value;
				Validate();
				}
		    }
		
		public bool IsValid 
			{
			get {return bValid;}
			}
		public bool Ambiente 
			{
			get	{return bAmbiente;}
			set	{
				ClearContorni();
				bAmbiente = true;
				Validate();
				}
			}
		public double DimCaratteristica 
			{
			get { return dimcar; }
			}
		public static double FrazioneEpsilon = 1e-3;	// Usata per il calcolo di un punto di poco fuori dalla superficie
		#endregion
		#region COSTRUTTORI
			/// <summary>
			/// Costruttore
			/// </summary>
		public CorpoOttico() 
			{
			contorni = new LinkedList<Contorno>();
			n = 1.0;
			bValid = false;
			bAmbiente = false;
			}
			/// <summary>
			/// Costruttore
			/// </summary>
			/// <param name="n_rifraz"></param>
		public CorpoOttico(double n_rifraz) : this() 
			{
			this.n = n_rifraz;
			}
		public CorpoOttico(Materiale mat) : this()
			{
			this.materiale = mat;
			}
		#endregion
		#region FUNZIONI
			/// <summary>
			/// Verifica e corregge dove possibile
			/// </summary>
			/// <returns></returns>
		public void Validate() 
			{
			Check[] ops;									// Array di delegate
			bValid = true;
			CalcolaDimCar();
			if(bAmbiente)									// Liste con i controlli se ambiente o corpo ottico finito
				ops = new Check[] {CheckRifrazione};
			else
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
									if( x.t1 > Double.Epsilon)				// e` gia` sul contorno
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
		public Intersection TrovaPrimaIntersezione(Line2D lin) 
			{
			List<Intersection> li = this.TrovaIntersezioniPositive(lin);
			if(li.Count > 0)
				return li[0];
			else
				return new Intersection(null, 0, 0, null, null);
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
		#region PROTECTED FUNC
		protected bool CheckNumTratti() 
			{
			if(contorni.Count < CorpoOttico.nTrattiMin)		// Controlla contorni.Count >= 3
				return false;
			else
				return true;
			}
		protected bool CheckConnesso() 
			{
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
		protected bool CheckRifrazione() 
			{
			if(n <= Double.Epsilon)							// Controlla n e corregge
				n = 1.0;
			return true;
			}
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




			Raggio CalcolaRaggio(Raggio rIn, CorpoOttico ambiente)
				{
				Raggio rout = null;
				if(ambiente==null)									// controlli iniziali
					return rout;
				if((!ambiente.Ambiente) || (!ambiente.IsValid))
					return rout;
				Intersection fint = TrovaPrimaIntersezione(rIn);	// Trova prima intersezione
				if(fint != null)
					{
					List<Contorno> lc = Belongs(fint.p);			// Trova i contorni cui appartiene la prima intersezione
					if(lc.Count > 2)								// Se piu` di due tratti: errore
						throw new Exception("Intersezione unica di un raggio con piu` di due tratti, in CalcolaRaggio()");
					if(lc.Count == 2)								// Se due tratti: su vertica
						{
						if(Tangenti(lc[0].Tratto, lc[1].Tratto))	// Se tangenti, considero intersezione su uno dei due, equivalente
							{
							if(lc[0].Stato != lc[1].Stato)			// Se hanno uno stato superficiale diverso, elimina il raggio
								{
								lc.Clear();
								}
							else									// altrimenti
								lc.Remove(lc[1]);					// elimino l'ultimo contorno
							}
						}
					if(lc.Count == 1)								// Se una sola intersezione
						{
						Point2D versoreIn = rIn.Vector();			// Versori entrante (Raggio gia` normalizzato) e normale.
						Point2D versoreNorm = Function2D.VersorOut(fint.p, lc[0].Tratto, rIn.Point(fint.t1 - dimcar*FrazioneEpsilon));
						switch(lc[0].Stato)
							{
							case StatoSuperficie.Opaca:
								{
								// Non fa nulla, raggio assorbito, nessun raggio in uscita
								break;
								}
							case StatoSuperficie.Riflettente:
								{
								rout = new Raggio(new Line2D(fint.p, Ottica.Riflesso(versoreIn, versoreNorm), true));
								rout.CorpoAttuale = rIn.CorpoAttuale;
								break;
								}
							case StatoSuperficie.Trasparente:
								{
								CorpoOttico co_in, co_out;			// Corpi ottici del raggio in ingresso ed uscita
								co_in = rIn.CorpoAttuale;
								if(co_in == this)					// Se il raggio entrante si trova nel corpo ottico
									{
									co_out = ambiente;				// quello uscente nell'ambiente
									}
								else								// se no, da ambiente...
									{
									co_out = this;					// a corpo attuale
									}
								Point2D vrifr = Ottica.Rifratto(versoreIn, versoreNorm, co_in.IndiceRifrazione,  co_out.IndiceRifrazione);
								if(vrifr != null)
									{
									rout = new Raggio(new Line2D(fint.p, vrifr, true));
									// throw new Exception("Manca determinazione se il raggio parte dall'esterno o dall'interno, per scegliere n1->n2 o n2->n1 ");
									rout.CorpoAttuale = co_out;
									}
								break;
								}
							}
						}
					}
				return rout;
				}



		#endregion
		#endregion
		}	
	}