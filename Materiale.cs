using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;						// Per lettura e scrittura su file
using Fred68.Tools.Utilita;				// Per TokenString

namespace Fred68.Tools.Engineering
	{
	enum Materiali	{Utente, Acciaio, Alluminio, NumeroMaterialiStandard };
	class Materiale : Oggetto
		{
		protected static readonly new string descrittore = "MAT";
		protected static readonly double nu_min = -1.0;
		protected static readonly double nu_max = 0.5;
		protected static readonly double nu_default = 0.3;
		protected static readonly double E_default = 210e9;
		protected static readonly double n_default = 1.0;		

		protected static readonly Materiali mat_default = Materiali.Acciaio;

		protected Materiali mat;				// Nome materiale
		protected double E_;					// Modulo di elasticita`
		protected double nu_;					// Modulo di Poisson nu=(E/2G)-1
		protected double G_;					// Modulo di elasticita` trasversale G = E/(2*(nu+1))
		protected double alfa_;					// Coefficiente di dilatazione termica lineare
		protected double sigmarp_;				// Tensione di snervamento o limite di proporzionalita` Rp 0.2
		//protected double n_;					// Indice di rifrazione
		#region PROPRIETA
		public double E							// Modulo elastico (Fe=210e9)
			{
			get		{
					return E_;
					}
			set		{
					E_= value;					// Imposto E...
					//G_ = E_ / (2*( nu_ + 1));		// ...e ricalcolo G senza modificare nu
					mat = Materiali.Utente;		// Reimposto materiale utente
					//Nome = mat.ToString();					
					}

			}
		public double nu						// Mod. di Poisson 
			{									// (Fe=0.3) http://it.wikipedia.org/wiki/Modulo_di_Poisson
			get		{
					return nu_;
					}
			set		{
					nu_ = value;				
					mat = Materiali.Utente;		
					}
			}
		public double G							// Modulo tangenziale 
			{
			get		{
					return G_;
					}
			set		{							
					G_ = value;
					mat = Materiali.Utente;		
					}		
			}
		public double Alfa						// Coeff. dilatazione termica lineare (Fe=11e-6)
			{
			get		{
					return alfa_;
					}
			set		{
					alfa_ = value;				
					mat = Materiali.Utente;		
					}
			}
		public double SigmaRp					// Coeff. dilatazione termica lineare (Fe=11e-6)
			{
			get		{
					return sigmarp_;
					}
			set		{
					sigmarp_ = value;			
					mat = Materiali.Utente;		
					}
			}
		//public double nRifrazione
		//    {
		//    get		{
		//            return n_;
		//            }
		//    set
		//            {
		//            n_ = value;
		//            if(n_ <= 0.0)
		//                n_ = n_default;
		//            mat = Materiali.Utente;
		//            }
		//    }
		#endregion
		#region COSTRUTTORI
		public Materiale()	: base(NonValido, NomeStandard)
			{
			mat = Materiali.Utente;			// Materiale di default e` l'acciaio
			E_ = 0.0;						
			nu_ = 0.3;
			alfa_ = 0.0;
			G_ = E_ / (2*( nu_ + 1));
			tipo = TipoOggetto.Materiale;
			sigmarp_ = 0.0;
			Nome = mat.ToString();
			}
		public Materiale(Materiali m)
			{
			switch(m)
				{
				case Materiali.Utente:				// Se Utente non modifica nulla
					break;
				default:							// Se non riconosciuto, imposta l'acciaio
				case Materiali.Acciaio:
					{
					mat = Materiali.Acciaio;
					E_ = 210e9;						// Materiale di default e` l'acciaio
					nu_ = 0.3;
					alfa_ = 11e-6;
					G_ = E_ / (2 * (nu_ + 1));
					sigmarp_ = 220e6;
					break;
					}
				case Materiali.Alluminio:
					{
					mat = Materiali.Alluminio;
					E_ = 71e9;
					nu_ = 0.3;
					alfa_ = 24e-6;
					G_ = E_ / (2 * (nu_ + 1));
					sigmarp_ = 100e6;
					break;
					}
				}
			Nome = mat.ToString();
			tipo = TipoOggetto.Materiale;
			}
		#endregion
		#region FUNZIONI
		public void CopiaCaratteristicheDa(Materiale m)		// Copia le caratteristiche ed il nome, ma non numero, id ecc...
			{
			Nome = m.Nome;
			E = m.E;
			nu = m.nu;
			G = m.G;
			Alfa = m.Alfa;
			SigmaRp = m.SigmaRp;
			}
		public void RicalcolaProprietaMancanti()			// Ricalcola le proprieta` nulle
			{
			CorreggiProprieta();									// Prima corregge le proprieta` fuori limite
			if( E == 0.0 )											// Se E nullo, lo calcola con G e nu
				E_ = G_ * (2*( nu_ + 1));
			if( G == 0.0 )											// Se G nullo, lo ricalcola con E e nu
				G_ = E_ / (2 * (nu_ + 1));
			if (nu == 0.0)										// Se nu = nu_minimo, lo ricalcola ricalcola con E e G
				nu_ = (E_ / (2 * G_)) - 1;	
			return;
			}
		public bool VerificaProprieta()						// Controlla le proprieta` non nulle
			{
			bool ok = true;
			if( (E < 0.0) || (nu < nu_min) || (nu > nu_max) || (G < 0.0)  || (SigmaRp < 0.0))
				ok = false;
			return ok;
			}
		public void CorreggiProprieta()						// Corregge le proprieta` fuori limite
			{
			if( E < 0.0 )									// Azzera i valori fuori limite
				E_ = 0.0;
			if( G < 0.0 )
				G_ = 0.0;
			if( SigmaRp < 0.0 )
				sigmarp_ = 0.0;
			if ((nu < nu_min) || (nu > nu_max))				// Imposta nu a 0.0, se fuori limite
				nu_ = 0.0;
			if( (E == 0.0) && (G == 0.0) )					// Se E e G nulli, imposta E al default
				E_ = E_default;
			if( (G == 0.0) && (nu == 0.0))					// Se G nullo e nu nullo...
				nu_ = nu_default;							// ...imposta nu al default (0.3)
			return;
			}
		#endregion
		#region IO SU STREAM
		public new bool Scrivi(StreamWriter sw)
			{
			sw.Write(descrittore); sw.Write(separatore);		// Scrive i dati
			sw.Write(nID); sw.Write(separatore);
			sw.Write(nome); sw.Write(separatore);
			sw.Write(numero); sw.Write(separatore);				// Non scrive il materiale di base
			sw.Write(E); sw.Write(separatore);
			sw.Write(nu); sw.Write(separatore);
			sw.Write(G); sw.Write(separatore);
			sw.Write(Alfa); sw.Write(separatore);
			sw.Write(SigmaRp); sw.Write(separatore);			
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
						case 0:									// Descrittore
							if (s != descrittore)				// Se oggetto non riconosciuto
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
						case 4:										// Legge i dati: E...
							if (double.TryParse(s, out dtmp))				
								E_ = dtmp;								// Li inserisce direttamente senza usare le proprieta`
							break;										// altrimenti puo` alterarne i valori
						case 5:
							if (double.TryParse(s, out dtmp))		// nu...
								nu_ = dtmp;
							break;
						case 6:										// G...
							if (double.TryParse(s, out dtmp))
								G_ = dtmp;
							break;
						case 7:										// alfa
							if (double.TryParse(s, out dtmp))
								alfa_ = dtmp;
							break;
						case 8:										// sigma Rp
							if (double.TryParse(s, out dtmp))
								sigmarp_ = dtmp;
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
/* nu:
gomma ~ 0,50 
oro 0,42 
magnesio 0,35 
titanio 0,34 
rame 0,33 
lega d'alluminio 0,33 
acciaio inossidabile 0,30-0,31 
acciaio 0,27-0,30 
ghisa 0,21-0,26 
cemento 0,20 
vetro 0,18-0,30 
*/