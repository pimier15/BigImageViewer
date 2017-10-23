﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpeedyCoding;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;

namespace PLImgViewer
{
	/// <summary>
	/// Interaction logic for EmdFileExport.xaml
	/// </summary>
	public partial class EmdFileExport : Window
	{
		public EmdFileExport()
		{
			InitializeComponent();
		}

		public List<EmdInfoData> EmdData;
		public int TrackLimitation;
		public int BlockWidth;

		double progress;
		double currentcount;
		object key = new object();


		public string Basepath = "";
		public bool IsRunning = false;
		public Task ProgressTask;
		public bool ProgressRun = true;

		private async void btnEmdLoad_Click( object sender , RoutedEventArgs e )
		{
			rlreverssed = chbRL.IsChecked;
			topbotrev = chbTB.IsChecked;

			progress = 0.0;
			Stopwatch stwtotal = new Stopwatch();
			
			if ( !IsRunning )
			{
				try
				{
					

					lblSTatus.Content = "Running";
					IsRunning = true;
					OpenFileDialog ofd = new OpenFileDialog();
					string emdpath = "";

					if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
					{
						stwtotal.Start();
						emdpath = ofd.FileName;
						Basepath = System.IO.Path.GetDirectoryName( emdpath );
						progress = 0.0;
						ProgressTask = Task.Run( () => {

							while ( ProgressRun )
							{
								lblProgress.Dispatcher.BeginInvoke( ( Action )( () => lblProgress.Content = progress.ToString("##.#") ) );
								Thread.Sleep( 500 );
							}
						} );
					}
					else
					{
						IsRunning =false;
						return;
					}


					Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
					lblProgress.Content = "0";

					Stopwatch stw = new Stopwatch();
					Task task1 = null;
					if ( false )
					{
						task1 = Task.Factory.StartNew( () =>
						{
							progress = 1.0;


							var bits8list = File.ReadAllBytes( emdpath );

							var oneblock = bits8list.Skip(32768).Take(224).ToArray();
							File.WriteAllBytes(@"D:\03JobPro\2017\013_Polygon\temp\Block.csv" , oneblock);

							ushort[] bits16list = new ushort[bits8list.Length / 2];
							Buffer.BlockCopy(bits8list , 0 , bits16list,0,bits8list.Length);

							uint[] bits32list = new uint[bits8list.Length / 4];
							Buffer.BlockCopy(bits8list , 0 , bits32list,0,bits8list.Length);

							//UInt64[] bits64list = new UInt64[bits8list.Length / 8];
							//Buffer.BlockCopy(bits32list , 0 , bits64list,0,bits8list.Length);

							var indices = bits32list.IndicesOf(x => x == 35192831)
												.Select( x => x)
												.ToArray<int>();



							progress = 2.0;


							var infos = indices.Select(x =>
							new
							{
								TrackNum = (int)bits32list[x+3]
								  ,
								BlockNum = (int)bits32list[x+1]
								   ,
								DataCount =   (int)bits32list[x+5]
								   ,
								Index = x
							}) // x is Header indices
                                    .ToList(); // Ok Double Check

							var threeblock = infos.Take(20);


							progress = 7.0;

							StringBuilder stb = new StringBuilder();
							infos.ActLoop( x =>
							{
								stb.Append( x.TrackNum.ToString() );
								stb.Append( ',' );
								stb.Append( x.BlockNum.ToString() );
								stb.Append( ',' );
								stb.Append( x.Index.ToString() );
								stb.Append( ',' );
								stb.Append( x.DataCount.ToString() );
								stb.Append( Environment.NewLine );

							}
						);

							var dir = System.IO.Path.GetDirectoryName( emdpath );
							File.WriteAllText( dir +"\\"+ "info.csv" , stb.ToString() );

							progress = 10.0;

							double step  = 70.0/infos.Count;

							var swapped = infos.Select( (x , i) =>
						 bits16list.Skip(x.Index   ).Take( (int)x.DataCount).Select(s => (byte)(s*255.0/ 65536.0)).ToArray().Act(y => progress += step)
						 ).ToList();

							var infoWithData =
								infos.Zip( swapped ,(f,s) => new EmdInfoData( f.TrackNum , f.BlockNum, s )
								).ToList(); // [( TrackNum , BlockNum , Data)]

							//bitmaps [ i ] = CopyDataToBitmap( oneTrackdata , w , trackdatas.Count() );
							//var counter = infoWithData.Where( x => x.TrackNum == 0);
							//Bitmap tempimg = new Bitmap();



							Console.WriteLine( "Step7 : " + stw.ElapsedMilliseconds );
							stw.Restart();

							var ordDataInfo = infoWithData
								.OrderBy( x => x.TrackNum )
								.ThenBy( x => x.BlockNum)
								.ToList(); // [( TrackNum , BlockNum , Data )] ordered by track and block number

							var trackLimit = infoWithData.Last().TrackNum;

							BlockWidth = infoWithData.First().Data.Count();
							EmdData = ordDataInfo;
							TrackLimitation = trackLimit + 1;
						} , TaskCreationOptions.LongRunning);

					}
					else
					{
						
							progress = 1.0;


							var bits8list = File.ReadAllBytes( emdpath );

							var oneblock = bits8list.Skip(32768).Take(224).ToArray();
							File.WriteAllBytes(@"D:\03JobPro\2017\013_Polygon\temp\Block.csv" , oneblock);

							ushort[] bits16list = new ushort[bits8list.Length / 2];
							Buffer.BlockCopy(bits8list , 0 , bits16list,0,bits8list.Length);

							uint[] bits32list = new uint[bits8list.Length / 4];
							Buffer.BlockCopy(bits8list , 0 , bits32list,0,bits8list.Length);

							//UInt64[] bits64list = new UInt64[bits8list.Length / 8];
							//Buffer.BlockCopy(bits32list , 0 , bits64list,0,bits8list.Length);

							var indices = bits32list.IndicesOf(x => x == 35192831)
												.Select( x => x)
												.ToArray<int>();



							progress = 2.0;


							var infos = indices.Select(x =>
							new
							{
								TrackNum = (int)bits32list[x+3]
								  ,
								BlockNum = (int)bits32list[x+1]
								   ,
								DataCount =   (int)bits32list[x+5]
								   ,
								Index = x
							}) // x is Header indices
                                    .ToList(); // Ok Double Check

							var threeblock = infos.Take(20);


							progress = 7.0;

							StringBuilder stb = new StringBuilder();
							infos.ActLoop( x =>
							{
								stb.Append( x.TrackNum.ToString() );
								stb.Append( ',' );
								stb.Append( x.BlockNum.ToString() );
								stb.Append( ',' );
								stb.Append( x.Index.ToString() );
								stb.Append( ',' );
								stb.Append( x.DataCount.ToString() );
								stb.Append( Environment.NewLine );

							}
						);

							var dir = System.IO.Path.GetDirectoryName( emdpath );
							File.WriteAllText( dir +"\\"+ "info.csv" , stb.ToString() );

							progress = 10.0;

							double step  = 70.0/infos.Count;

							var swapped = infos.Select( (x , i) =>
						 bits16list.Skip(x.Index   ).Take( (int)x.DataCount).Select(s => (byte)(s*255.0/ 65536.0)).ToArray().Act(y => progress += step)
						 ).ToList();

							var infoWithData =
								infos.Zip( swapped ,(f,s) => new EmdInfoData( f.TrackNum , f.BlockNum, s )
								).ToList(); // [( TrackNum , BlockNum , Data)]

							//bitmaps [ i ] = CopyDataToBitmap( oneTrackdata , w , trackdatas.Count() );
							//var counter = infoWithData.Where( x => x.TrackNum == 0);
							//Bitmap tempimg = new Bitmap();



							Console.WriteLine( "Step7 : " + stw.ElapsedMilliseconds );
							stw.Restart();

							var ordDataInfo = infoWithData
								.OrderBy( x => x.TrackNum )
								.ThenBy( x => x.BlockNum)
								.ToList(); // [( TrackNum , BlockNum , Data )] ordered by track and block number

							var trackLimit = infoWithData.Last().TrackNum;

							BlockWidth = infoWithData.First().Data.Count();
							EmdData = ordDataInfo;
							TrackLimitation = trackLimit + 1;
						} , TaskCreationOptions.LongRunning);

					}

					await task1;
					await ToImageList( Basepath , TrackLimitation , BlockWidth );
					Mouse.OverrideCursor = null;
					IsRunning = false;
					lblSTatus.Content = "Waiting";
					progress = 100.0;

				

				}
				catch ( Exception )
				{
					System.Windows.MessageBox.Show( ".emd file is broken. please Check .emd File  " );
				}

				var totaltime = (stwtotal.ElapsedMilliseconds/1000).ToString();
				lblSTatus.Content = "Total Running Time : " + totaltime + "(sec)";
				stwtotal.Stop();
				stwtotal.Reset();
			}

		}

		public Bitmap CopyDataToBitmap( byte [ ] data , int w, int h)
		{
			//Here create the Bitmap to the know height, width and format
			Bitmap bmp = new Bitmap( w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

			//Create a BitmapData and Lock all pixels to be written 
			BitmapData bmpData = bmp.LockBits(
					   new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
					   ImageLockMode.WriteOnly, bmp.PixelFormat);

			//Copy the data from the byte array into BitmapData.Scan0
			Marshal.Copy( data , 0 , bmpData.Scan0 , data.Length );


			//Unlock the pixels
			bmp.UnlockBits( bmpData );


			//Return the bitmap 
			return bmp;
		}


		bool? rlreverssed = false;
		bool? topbotrev = false;

		Task<bool> ToImageList( string basepah , int tracknum , int w )
		{
			return Task<bool>.Run( () =>
		  {
		  try
		  {
			  var totalnum = EmdData.Count();
			  byte[][] datalist = new byte[totalnum][];
			  Bitmap[] bitmaps = new Bitmap[tracknum];

			  Console.WriteLine( "Image start" );
			  double step = 18.0/tracknum;

				  //Parallel.For( 0 , tracknum , i =>
				  //{
				  //  try
				  //  {
				  //	  List<byte[]> trackdatas;
				  //	  if ( rlreverssed == true )
				  //	  {
				  //		  if ( i % 2 == 0 ) trackdatas = EmdData.Where( x => x.TrackNum == i )
				  //												  .Select( x => x.Data )
				  //												  .ToList();
				  //		  else trackdatas = EmdData.Where( x => x.TrackNum == i )
				  //									 .Select( x => x.Data.Reverse().ToArray() )
				  //									 .ToList();
				  //	  }
				  //	  else
				  //	  {
				  //		  trackdatas = EmdData.Where( x => x.TrackNum == i ).Select( x => x.Data ).ToList();
				  //	  }
				  //
				  //	  byte[] oneTrackdata = new byte[] { };
				  //
				  //	  if ( i % 2 == 0 ) oneTrackdata = trackdatas.Aggregate( ( f , s ) => f.Concat( s ).ToArray() );
				  //	  else
				  //	  {
				  //		  if ( topbotrev == true ) trackdatas.Reverse();
				  //		  oneTrackdata = trackdatas.Aggregate( ( f , s ) => f.Concat( s ).ToArray() );
				  //	  }
				  //	  bitmaps [ i ] = CopyDataToBitmap( oneTrackdata , w , trackdatas.Count() );
				  //  }
				  //  finally
				  //  {
				  //	  progress += step;
				  //  }
				  //
				  //
				  //} );


				  for ( int i = 0 ; i < tracknum ; i++ )
				  {
					  try
					  {
						  List<byte[]> trackdatas;
						  if ( rlreverssed == true )
						  {
							  if ( i % 2 == 0 ) trackdatas = EmdData.Where( x => x.TrackNum == i )
																	  .Select( x => x.Data )
																	  .ToList();
							  else trackdatas = EmdData.Where( x => x.TrackNum == i )
														 .Select( x => x.Data.Reverse().ToArray() )
														 .ToList();
						  }
						  else
						  {
							  trackdatas = EmdData.Where( x => x.TrackNum == i ).Select( x => x.Data ).ToList();
						  }

						  byte[] oneTrackdata = new byte[] { };

						  if ( i % 2 == 0 ) oneTrackdata = trackdatas.Aggregate( ( f , s ) => f.Concat( s ).ToArray() );
						  else
						  {
							  if ( topbotrev == true ) trackdatas.Reverse();
							  oneTrackdata = trackdatas.Aggregate( ( f , s ) => f.Concat( s ).ToArray() );
						  }
						  bitmaps [ i ] = CopyDataToBitmap( oneTrackdata , w , trackdatas.Count() );
					  }
					  finally
					  {
						  progress += step;
					  }

				  }


				  for ( int i = 0 ; i < bitmaps.GetLength( 0 ) ; i++ )
					{
						bitmaps [ i ].Save( basepah + "\\" + i.ToString( "D3" ) + ".bmp" );
					}

					bitmaps.Select( x => new Image<Bgr , byte>( x ) )
						   .Aggregate( ( f , s ) => f.ConcateHorizontal( s ) )
						   .Save( basepah + "\\" + "Full.bmp" );
					return true;
				}
				catch ( Exception ex )
				{
					System.Windows.MessageBox.Show( "Data is too big to create full stitched image" );
					return false;
				}
			} );
		}



		#region Not Use
		private async void btnEmdExtract_Click(string basepath)
		{

			try
			{
				await Task.Run( () =>
				{
					this.Dispatcher.BeginInvoke( ( Action )( () => Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait ) );
					string dirpath = basepath + "\\";
					for ( int i = 0 ; i < TrackLimitation ; i++ )
					{
						string path = dirpath + i.ToString("D3") +".dat";

						Stream outStream = new FileStream(path, FileMode.Create);


						var trackdata = EmdData.Where( x => x.TrackNum == i ).ToList();
						for ( int j = 0 ; j < trackdata.Count() ; j++ )
						{
							var datacount =  trackdata [ j ].Data.Count();
							outStream.Write( trackdata [ j ].Data , 0 , datacount );

						}
						outStream.Dispose();
						if ( i == 0 ) Height = trackdata.Count();
					}

					StringBuilder stb = new StringBuilder();
					//stb.Append( "TrackLimitation" );
					//stb.Append( "," );
					//stb.Append( "BlockWidth" );
					//stb.Append( "," );
					//stb.Append( "Width" );
					//stb.Append( "," );
					//stb.Append( "Height" );
					//stb.Append( Environment.NewLine );
					stb.Append( TrackLimitation );
					stb.Append( "," );
					stb.Append( BlockWidth );
					stb.Append( "," );
					stb.Append( Width );
					stb.Append( "," );
					stb.Append( Height );
					File.WriteAllText( dirpath + "info.txt" , stb.ToString() );

				} );


			}
			catch ( Exception )
			{
				System.Windows.MessageBox.Show( "Loaded .emd file have some problem. Load .emd file again please. " );
			}
			
			
		}

		
		private async void btnToCsv_Click( object sender , RoutedEventArgs e )
		{
			if ( !IsRunning )
			{
				try
				{

					lblSTatus.Content = "Running";
					IsRunning = true;
					FolderBrowserDialog ofd = new FolderBrowserDialog();
					ofd.SelectedPath = Basepath;
					if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
					{
						await Task.Run( () =>
						{
							this.Dispatcher.BeginInvoke( ( Action )( () => Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait ) );
							string dirpath = ofd.SelectedPath + "\\";

							for ( int i = 0 ; i < TrackLimitation ; i++ )
							{
								string path = dirpath + i.ToString("D3") +".csv";
								StringBuilder stb = new StringBuilder();


								var trackdata = EmdData.Where( x => x.TrackNum == i ).ToList();
								for ( int j = 0 ; j < trackdata.Count() ; j++ )
								{
									trackdata [ j ].Data.ActLoop( a =>
									{
										stb.Append( a.ToString() );
										stb.Append( ',' );
									} );
									stb.Append( Environment.NewLine );
								}
								if ( i == 0 ) Height = trackdata.Count();
								File.WriteAllText( path , stb.ToString() );
							}


						} );
					}
					else
					{
						IsRunning = false;
					}

					Mouse.OverrideCursor = null;
					IsRunning = false;
					lblSTatus.Content = "Waiting";
				}
				catch ( Exception )
				{
					System.Windows.MessageBox.Show( "Loaded .emd file have some problem. Load .emd file again please. " );
				}
			}
		}
		#endregion	
	}

	public static class et {
		public static void SaveCsv( this List<string[]> src )
		{
			StringBuilder sr = new StringBuilder();
			foreach ( var item in src )
			{
				foreach ( var el in item )
				{
					sr.Append( el );
					sr.Append( "," );
					
				}
				sr.Append( Environment.NewLine );

			}
			File.WriteAllText( @"D:\03JobPro\2017\013_Polygon\0914\text.csv" , sr.ToString() );
		}

		public static void SaveCsv2( this List<string [ ]> src )
		{
			StringBuilder sr = new StringBuilder();
			StringBuilder sr2 = new StringBuilder();
			foreach ( var item in src )
			{
				for ( int i = 0 ; i < item.GetLength(0)/2 ; i++ )
				{
					var one = item[i*2];
					var tow = item[i*2+1];

					var eln = item[i*2] + item[i*2+1];
					var eln2 = item[i*2+1] + item[i*2];

					var elnn = Convert.ToInt32(eln , 16 );
					var elnn2 = Convert.ToInt32(eln2 , 16 );
					sr.Append( elnn );
					sr.Append( "," );
					sr2.Append( elnn2 );
					sr2.Append( "," );

				}

			
				sr.Append( Environment.NewLine );
				sr2.Append( Environment.NewLine );

			}
			File.WriteAllText( @"D:\03JobPro\2017\013_Polygon\0914\text.csv" , sr.ToString() );
			File.WriteAllText( @"D:\03JobPro\2017\013_Polygon\0914\textrr.csv" , sr2.ToString() );
		}


	}

}
