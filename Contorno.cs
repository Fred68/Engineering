using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Fred68.Tools.Matematica;
using Fred68.Tools.Grafica;

namespace Fred68.Tools.Engineering
	{
	#pragma warning disable 1591
	/// <summary> Stato di una superficie </summary>
	public enum StatoSuperficie {Trasparente, Riflettente, Opaca};
	#pragma warning restore 1591
	/// <summary> Contorno </summary>
	public class Contorno : IPlot
		{
		#pragma warning disable 1591
		#region PROTECTED
		/// <summary>
		/// Oggetto con il segmento (Line2D, Arc2D...)
		/// </summary>
		protected Tratto obj;
		/// <summary>
		/// Tipo di superficie
		/// </summary>
		protected StatoSuperficie stat;
		/// <summary>
		/// Superficie a contatto con altro corpo
		/// (Ignora indice di rifrazione del mezzo globale)
		/// </summary>
		protected bool aContatto;
		protected void Inizializza()
			{
			stat = StatoSuperficie.Trasparente;
			aContatto = false;
			}
		#endregion
		#pragma warning restore 1591
		#region COSTRUTTORI
		/// <summary>
		/// Costruttore generico, non ammesso.
		/// </summary>
		public Contorno() 
			{
			throw new Exception("Non ammesso costruttore senza argomenti");
			}
		//public Contorno(Line2D line, StatoSuperficie stato = StatoSuperficie.Trasparente) 
		//    {
		//    Inizializza();
		//    obj = line;
		//    stat = stato;
		//    }
		//public Contorno(Arc2D arc, StatoSuperficie stato = StatoSuperficie.Trasparente) 
		//    {
		//    Inizializza();
		//    obj = arc;
		//    stat = stato;
		//    }
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="tr"></param>
		/// <param name="stato"></param>
		public Contorno(Tratto tr, StatoSuperficie stato = StatoSuperficie.Trasparente)
			{
			Inizializza();
			obj = tr;
			stat = stato;
			}
		#endregion
		#region PROPRIETA
		/// <summary>
		/// Stato superficie (enum StatoSuperficie)
		/// </summary>
		public StatoSuperficie Stato
			{
			get {return stat;}
			set {stat = value;}
			}
		/// <summary>
		/// Tratto corrispondente
		/// </summary>
		public Tratto Tratto
			{
			get {return obj;}
			}

		/// <summary>
		/// Superficie e contatto
		/// Ignora n rifrazione del mezzo globale fino al prossimo oggetto
		/// </summary>
		public bool Acontatto
			{
			get {return aContatto;}
			set {aContatto = value;}
			}
		#endregion
		#region FUNZIONI
		/// <summary>
		/// Plot
		/// </summary>
		/// <param name="dc"></param>
		/// <param name="fin"></param>
		/// <param name="penna"></param>
		public void Plot(Graphics dc, Finestra fin, Pen penna)
			{
			this.Tratto.Plot(dc,fin,penna);
			}
		/// <summary>
		/// Aggiunge alla display list
		/// </summary>
		/// <param name="displaylist"></param>
		/// <param name="penna"></param>
		public void Display(DisplayList displaylist, int penna)
			{
			this.Tratto.Display(displaylist,penna);
			}
		#endregion
		}
	}