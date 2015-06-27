using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Fred68.Tools.Matematica;
using Fred68.Tools.Grafica;

namespace Fred68.Tools.Engineering
	{
	/// <summary> Raggio </summary>
	public class Raggio : Line2D 
		{
		/// <summary>
		/// Materiale in cui si trova il raggio
		/// </summary>
		protected MaterialeOttico corpoAttuale;
		/// <summary>
		/// Parametro t inizio proiezione raggio
		/// </summary>
		protected double t1r;
		/// <summary>
		/// Parametro t fine proiezione raggio
		/// </summary>
		protected double t2r;
		/// <summary>
		/// Lunghezza d'onda
		/// in nm (1e-9 m)
		/// </summary>
		protected double lambda;
		
		#region COSTRUTTORI
		/// <summary>
		/// Costruttore
		/// </summary>
		public Raggio() : base()
			{
			corpoAttuale = null;
			t1r = 0.0;
			t2r = 1.0;
			lambda = 0.0;
			}
		/// <summary>
		/// Costruttore da linea e lunghezza d'onda
		/// </summary>
		/// <param name="l">Linea</param>
		/// <param name="lambda">Lunghezza d'onda in nm (1e-9 m)</param>
		public Raggio(Line2D l, double lambda) : base(l)
			{								// Chiama il costruttore della classe base (che comprende Line2D.Validate()
			t1r = 0.0;
			t2r = 1.0;
			this.lambda = lambda;
			this.Validate();				// poi chiama Raggio.Validate()
			}							
		#endregion
		#region PROPRIETA
		/// <summary>
		/// Materiale in cui si trova il raggio
		/// </summary>
		public MaterialeOttico CorpoAttuale 
			{
			get { return corpoAttuale; }
			set { corpoAttuale = value; }
			}
		/// <summary>
		/// Parametro t1 proiezione raggio
		/// </summary>
		public double T1r
			{
			get { return t1r; }
			set { t1r = value; }
			}
		/// <summary>
		/// Parametro t2 proiezione raggio
		/// </summary>
		public double T2r
			{
			get { return t2r; }
			set { t2r = value; }
			}
		/// <summary>
		/// Lunghezza d'onda
		/// </summary>
		public double Lambda
			{
			get { return lambda;}
			set { lambda = value;}
			}
		#endregion
		#region FUNZIONI
		/// <summary>
		/// Corregge
		/// </summary>
		public new void Validate()		// Aggiunge dei controlli
		    {
		    Normalize();					// Chiama Normalize(). Line2D.Normalize() chiama a sua volta this.Validate().
											// Se funzione definita con override, il Line2D.Normalize richiamera` la versione this.Validate()
											// definita con override, quindi ciclo infinito.
											// Se la definisco con new, dalla classe base vedo solo il Validate li` definito
			// Se qualche errore, imposta valid = false;
			if(this.lambda < Double.Epsilon)
				this.valid = false;
		    }
		/// <summary>
		/// Plot, traccia raggio completo
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="fin"></param>
		/// <param name="penna"></param>
		public override void Plot(Graphics dc, Finestra fin, Pen penna)
			{
			if(IsValid)
				{
				Point start, end;
				//start = fin.Get(this.pStart);
				//end = fin.Get(this.pEnd);
				start = fin.Get(this.Point(this.T1r));
				end = fin.Get(this.Point(this.t2r));
				dc.DrawLine(penna,start,end);
				}
			}
		#endregion
		}
	}

