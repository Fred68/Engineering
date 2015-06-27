using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using Fred68.Tools.Grafica;
using Fred68.Tools.Matematica;

namespace Fred68.Tools.Engineering
	{
	/// <summary> Classe Lente </summary>
	public class Lente :  OggettoOttico 
		{
		/// <summary>Tipi di superficie</summary>
		public enum TipoSuperficie 
			{
			/// <summary>Superficie piana</summary>
			piana=0,
			/// <summary>Convessa</summary>
			convessa=1,
			/// <summary>Concava</summary>
			concava=-1
			};
		/// <summary>
		/// valore minimo
		/// </summary>
		public static double epsilon = 1e-6;
		/// <summary>
		/// Edge thickness
		/// </summary>
		protected double et;
		/// <summary>
		/// Center thickness
		/// </summary>
		protected double ct;
		/// <summary>
		/// Indice di rifrazione
		/// </summary>
		protected double n;
		/// <summary>
		/// Diametro lente
		/// </summary>
		protected double d;
		/// <summary>
		/// Raggio lente
		/// </summary>
		protected double rl;
		/// <summary>
		/// Superficie di ingresso
		/// </summary>
		protected TipoSuperficie tipo1;
		/// <summary>
		/// Raggio di ingresso
		/// </summary>
		protected double r1;
		/// <summary>
		/// Superficie di uscita
		/// </summary>
		protected TipoSuperficie tipo2;
		/// <summary>
		/// Raggio di uscita
		/// </summary>
		protected double r2;
		//protected bool bValid;		// Usato bool bValid della classe base
		/// <summary>
		/// Centri di curvatura
		/// </summary>
		protected double xcc1,xcc2;
		/// <summary>
		/// Raggi di curvatura con segno
		/// </summary>
		protected double rc1,rc2;
		/// <summary>
		/// Ascisse intersezione superfici lente con asse ottico
		/// </summary>
		protected double xlo1,xlo2;
		#pragma warning disable 1591

		public double ET	{										// Proprieta`
							get {return et;}
							set	{et = value; Validate();}
							}
		public double CT	{							
							get {return ct;}
							}			
		public double N		{
							get {return n;}
							set {n = value; Validate();}
							}
		public double D		{
							get {return d;}
							set { d = value; Validate(); }
							}
		public double RL	
							{
							get { return rl; }
							}					
		public TipoSuperficie TIPO1	{
									get {return tipo1;}
									set { tipo1 = value; Validate();}
									}
		public double R1	{
							get {return r1;}
							set { r1 = value; Validate(); }
							}
		public double XLO1	{
							get {return xlo1;}
							}
		public double XLO2	{
							get {return xlo2;}
							}
		public double XCC1	{
							get {return xcc1;}
							}
		public double XCC2	{
							get {return xcc2;}
							}
		public TipoSuperficie TIPO2	{
									get {return tipo2;}
									set { tipo2 = value; Validate();}
									}
		public double R2	{
							get { return r2; }
							set { r2 = value; Validate();}
							}				
		//public bool IsValid [era: VALIDA]	{							
		//                    get {return valida;}
		//                    }
		#pragma warning restore 1591
		/// <summary>
		/// Costruttore
		/// </summary>
		public Lente()
			{
			et = ct = 5;
			n = 1.4;
			d = rl = 20;
			tipo1 = TipoSuperficie.convessa;
			tipo2 = TipoSuperficie.piana;
			r1 = r2 = 100;
			xcc1 = xcc2 = 0;
			rc1 = rc2 = 0;
			xlo1 = xlo2 = 0;
			bValid = false;
			Validate();
			}
		/// <summary>
		/// Costruttore di copia
		/// </summary>
		/// <param name="lente"></param>
		public Lente(Lente lente) : base(lente.Nome)	
			{
			et = lente.et;					
			ct = lente.ct;
			n = lente.n;
			d = lente.d;
			rl = lente.rl;	
			tipo1 = lente.tipo1;
			r1 = lente.r1;
			tipo2 = lente.tipo2;
			r2 = lente.r2;
			bValid = lente.bValid;
			xcc1 = lente.xcc1;
			xcc2 = lente.xcc2;
			rc1 = lente.rc1;
			rc2 = lente.rc2;
			xlo1 = lente.xlo1;
			xlo2 = lente.xlo2;
			Validate();
			}
		/// <summary>
		/// Ricalcola
		/// </summary>
		public override void Validate()
			{
			double cf1 = 0.0;									// Segni dei raggi oppure zero
			double cf2 = 0.0;
			double d1q, d2q;									// Temporanee per verifica intersezione entro raggio lente
			bValid = false;										// Imposta come se errore
			cf1 = (int) tipo1;									// Estrae il segno equivalente al tipo
			cf2 = (int) tipo2;
			rl = d / 2.0;										// Calcola e verifica
			if( (r1 < epsilon) && (cf1 != 0.0))					// Se errore, esce
				{
				bValid = false;
				return;
				}
			if( (r2 < epsilon) && (cf2 != 0.0))
				{
				bValid = false;
				return;
				}
			if (rl < epsilon)
				{
				bValid = false;
				return;
				}
			d1q = Math.Abs(cf1) * (r1 * r1 - rl * rl);
			d2q = Math.Abs(cf2) * (r2 * r2 - rl * rl);
			if( (d1q < 0.0) || (d2q < 0.0))
				{
				bValid = false;
				return;
				}
			xcc1 = -et + cf1 * Math.Sqrt(Math.Abs(cf1) * (d1q));		// Centri di curvatura
			xcc2 = -cf2 * Math.Sqrt(Math.Abs(cf2) * (d2q));
			rc1 = r1 * cf1;												// Raggi di curvatura con segno
			rc2 = r2 * cf2;
			xlo1 = xcc1 - rc1;											// Ascisse sup. lente su asse ottico
			xlo2 = xcc2 + rc2;
			ct = xlo2 - xlo1;											// Center thickness
			if (ct < epsilon) 
				{
				bValid = false;
				return;
				}
			bValid = true;
			}
		/// <summary>
		/// Salva su stream
		/// </summary>
		/// <param name="stream">StreamWriter</param>
		/// <returns></returns>
		public bool SalvaStream(StreamWriter stream)
			{
			stream.WriteLine(string.Format("nome={0}", Nome));	// Scrive nello stream
			stream.WriteLine(string.Format("et={0}", et));		
			stream.WriteLine(string.Format("n={0}", n));		// Converte tutto in numero in virgola mobile
			stream.WriteLine(string.Format("d={0}", d));
			stream.WriteLine(string.Format("tipo1={0}", (int)tipo1));		
			stream.WriteLine(string.Format("r1={0}", r1));
			stream.WriteLine(string.Format("tipo2={0}", (int)tipo2));
			stream.WriteLine(string.Format("r2={0}", r2));
			stream.Flush();
			return true;
			}
		/// <summary>
		/// Carica da stream
		/// </summary>
		/// <param name="stream">StreamReader</param>
		/// <returns></returns>
		public bool CaricaStream(StreamReader stream)
			{
			while(!stream.EndOfStream)							// Percorre lo stream
				{
				string line = stream.ReadLine();				// Legge ogni riga dello stream
				int indx;
				if((indx = line.IndexOf("=")) != -1)			// La separa in due parti: prima e dopo il simbolo '='
					{
					string ini = line.Substring(0,indx);
					string fin = line.Substring(indx+1);
					double res = 0.0;
					if(ini=="nome")
						Nome = fin;
					else
						{
						if(double.TryParse(fin,out res))			// Legge il double dalla seconda parte
							{
							res = double.Parse(fin);
							}
						switch(ini)									// Imposta la variabile in base alla prima parte.
							{
							case "et":
								et = res;
								break;
							case "n":
								n = res;
								break;
							case "d":
								d = res;
								break;
							case "tipo1":
								tipo1 = (TipoSuperficie) res;
								break;
							case "r1":
								r1 = res;
								break;
							case "tipo2":
								tipo2 = (TipoSuperficie)res;
								break;
							case "r2":
								r2 = res;
								break;
							default:
								break; 
							}
						}
					}
				}
			Validate();										// Ricalcola e restituisce true se valida.
			return bValid;			
			}
		/// <summary>
		/// Nome descrittivo del materiale
		/// </summary>
		/// <returns></returns>
		public string GetNomeMateriale()
			{
			return "Materiale_"+this.Nome;
			}
		/// <summary>
		/// Nome descrittovo dell'oggetto
		/// </summary>
		/// <returns></returns>
		public string GetNomeCorpoOttico()
			{
			return "Lente_"+this.Nome;
			}

		/// <summary>
		/// Crea corpo ottico dalla lente nella posizione standard
		/// </summary>
		/// <returns></returns>
		//public CorpoOttico CreaCorpoOttico() 
		//    {
		//    #warning Creare funzione CreaCorpoOttivo(Transform2D)
		//    CorpoOttico co = null;
		//    if(this.IsValid)
		//        {
		//        Point2D pt1 = new Point2D(0, RL);						// Punti caratteristici
		//        Point2D pt2 = new Point2D(-ET, RL);
		//        Point2D pt3 = new Point2D(-ET, -RL);
		//        Point2D pt4 = new Point2D(0.0, -RL);
		//        Point2D ct1 = new Point2D(XCC1,0.0);
		//        Point2D ct2 = new Point2D(XCC2,0.0);

		//        Tratto sup1, sup2;											// Contorni
		//        Tratto bordo1, bordo2;
		//        if(TIPO1 == Lente.TipoSuperficie.piana)
		//            sup1 = new Line2D(pt2,pt3);
		//        else if(TIPO1 == Lente.TipoSuperficie.convessa)
		//            sup1 = new Arc2D(pt2,pt3,ct1,Arc2D.TrePunti.Estremi_e_Centro);
		//        else
		//            sup1 = new Arc2D(pt3,pt2,ct1,Arc2D.TrePunti.Estremi_e_Centro);
		//        if(TIPO2 == Lente.TipoSuperficie.piana)
		//            sup2 = new Line2D(pt1,pt4);
		//        else if(TIPO2 == Lente.TipoSuperficie.convessa)
		//            sup2 = new Arc2D(pt4,pt1,ct2,Arc2D.TrePunti.Estremi_e_Centro);
		//        else
		//            sup2 = new Arc2D(pt1,pt4,ct2,Arc2D.TrePunti.Estremi_e_Centro);
		//        bordo1 = new Line2D(pt1, pt2);
		//        bordo2 = new Line2D(pt3,pt4);

		//        string nome_materiale = GetNomeMateriale();									// Imposta i nomi
		//        string nome_lente = GetNomeCorpoOttico();
		//        MaterialeOttico mat_lente = new MaterialeOttico(nome_materiale, this.N);	// Crea il materiale
		//        co = new CorpoOttico(mat_lente,nome_lente);									// Crea il corpo ottico
		//        co.Add(new Contorno(bordo1, StatoSuperficie.Opaca));						// Aggiunge i contorni
		//        co.Add(new Contorno(sup1));
		//        co.Add(new Contorno(bordo2, StatoSuperficie.Opaca));
		//        co.Add(new Contorno(sup2));

		//        }			
		//    return co;
		//    }
		public CorpoOttico CreaCorpoOttico(Transform2D tr = null) 
			{
			CorpoOttico co = null;
			if(this.IsValid)
				{
				if(tr==null)
					{
					tr = new Transform2D(Matrix.Id(Transform2D.Dim2Dhom));
					}
				Point2D pt1 = tr.Transform(new Point2D(0, RL));				// Punti caratteristici
				Point2D pt2 = tr.Transform(new Point2D(-ET, RL));
				Point2D pt3 = tr.Transform(new Point2D(-ET, -RL));
				Point2D pt4 = tr.Transform(new Point2D(0.0, -RL));
				Point2D ct1 = tr.Transform(new Point2D(XCC1,0.0));
				Point2D ct2 = tr.Transform(new Point2D(XCC2,0.0));
				Tratto sup1, sup2;											// Contorni
				Tratto bordo1, bordo2;
				if(TIPO1 == Lente.TipoSuperficie.piana)
					sup1 = new Line2D(pt2,pt3);
				else if(TIPO1 == Lente.TipoSuperficie.convessa)
					sup1 = new Arc2D(pt2,pt3,ct1,Arc2D.TrePunti.Estremi_e_Centro);
				else
					sup1 = new Arc2D(pt3,pt2,ct1,Arc2D.TrePunti.Estremi_e_Centro);
				if(TIPO2 == Lente.TipoSuperficie.piana)
					sup2 = new Line2D(pt1,pt4);
				else if(TIPO2 == Lente.TipoSuperficie.convessa)
					sup2 = new Arc2D(pt4,pt1,ct2,Arc2D.TrePunti.Estremi_e_Centro);
				else
					sup2 = new Arc2D(pt1,pt4,ct2,Arc2D.TrePunti.Estremi_e_Centro);
				bordo1 = new Line2D(pt1, pt2);
				bordo2 = new Line2D(pt3,pt4);

				string nome_materiale = GetNomeMateriale();									// Imposta i nomi
				string nome_lente = GetNomeCorpoOttico();
				MaterialeOttico mat_lente = new MaterialeOttico(nome_materiale, this.N);	// Crea il materiale
				co = new CorpoOttico(mat_lente,nome_lente);									// Crea il corpo ottico
				co.Add(new Contorno(bordo1, StatoSuperficie.Opaca));						// Aggiunge i contorni
				co.Add(new Contorno(sup1));
				co.Add(new Contorno(bordo2, StatoSuperficie.Opaca));
				co.Add(new Contorno(sup2));

				}			
			return co;
			}
		
		#warning Lente.Plot() da scrivere
		/// <summary>
		/// DA SCRIVERE
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="fin"></param>
		/// <param name="penna"></param>
		public override void Plot(Graphics dc, Finestra fin, Pen penna)
			{
			
			}
		#warning Lente.Display() da scrivere
		/// <summary>
		/// DA SCRIVERE
		/// </summary>
		/// <param name="displaylist"></param>
		/// <param name="penna"></param>
		public override void Display(DisplayList displaylist, int penna)
			{
			}
		/// <summary>
		/// Restituisce prima intersezione positiva con una linea (sempre null)
		/// Interagisce con una linea (raggio) se trasformata in CorpoOttico
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
	}
