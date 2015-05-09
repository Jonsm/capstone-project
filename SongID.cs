using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

public class SongID : MonoBehaviour {
	public enum Genre {Alternative, Rock, Trance, Reggae, Rap, Ambient, Folk, 
		Electronic, Dubstep, House, Metal, Classical, Unknown};
	public Genre genre = Genre.Unknown;
	public string path = "/Assets/Binaries/SampleSongs/Power.mp3";
	private string api = "NKXIZAZCBRNKFIPE1";
	void Start(){

		StartCoroutine (Request(path));
		}
	public IEnumerator Request (string path) {
		//get artist and track title
		TagLib.File trk = TagLib.File.Create (path);
		TagLib.Tag tag = trk.GetTag(TagLib.TagTypes.Id3v2);
		string track = tag.Title;
		string artist = tag.FirstPerformer;
		if (artist == null)
				artist = tag.FirstComposer;
		artist = artist.Replace (" ", "%20");
		if (track != null && artist != null) {
			string link = "http://developer.echonest.com/api/v4/artist/terms?api_key="+ api +"&name=" + artist + "&format=json";
			WWW www = new WWW (link);
			yield return www;
			Debug.Log(www.text);
			string str = www.text;
			
			if (str != null) {
				//take the first match and find genre
				string str1 = "\"name\"";
				string noQuote = "\"";
				string genre = "\"([^\"]*)\"";
				Regex r = new Regex (str1 + ": " +genre);
				string id = r.Match (str).Groups [1].Value;
				Debug.Log (id);
			}
		}
		
		yield return null;
		
	}
	
	private Genre genreFromString (string str) {
		string lc = str.ToLower ();
		
		//more specific types first
		if (lc.Contains ("alternative") || lc.Contains ("indie"))
			return Genre.Alternative;
		else if (lc.Contains ("metal") || lc.Contains ("scream") || lc.Contains ("core"))
			return Genre.Metal;
		else if (lc.Contains ("reggae"))
			return Genre.Reggae;
		else if (lc.Contains ("rock"))
			return Genre.Rock;
		else if (lc.Contains ("trance"))
			return Genre.Trance;
		else if (lc.Contains ("rap") || lc.Contains ("hip hop") || lc.Contains ("hiphop"))
			return Genre.Rap;
		else if (lc.Contains ("ambient") || lc.Contains ("background"))
			return Genre.Ambient;
		else if (lc.Contains ("folk") || lc.Contains ("country") || lc.Contains ("blues"))
			return Genre.Folk;
		else if (lc.Contains ("house"))
			return Genre.House;
		else if (lc.Contains ("step") || lc.Contains ("bass") || lc.Contains ("bad"))
			return Genre.Dubstep;
		else if (lc.Contains ("electronic") || lc.Contains ("tech") || lc.Contains ("edm") ||
		         lc.Contains ("dance"))
			return Genre.Electronic;
		else if (lc.Contains ("classic") || lc.Contains ("baroque") || lc.Contains ("romantic"))
			return Genre.Classical;
		else return Genre.Unknown;
	}
	
}
