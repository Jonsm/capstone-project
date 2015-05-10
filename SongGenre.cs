using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

public class SongGenre : MonoBehaviour {
	public enum Genre {Alternative, Rock, Trance, Reggae, Rap, Ambient, Folk, 
		Electronic, Dubstep, House, Metal, Classical, Unknown};
	public Genre genre = Genre.Unknown;

	public IEnumerator Request (string path) {
		//get artist and track title
		TagLib.File trk = TagLib.File.Create (path);
		TagLib.Tag tag = trk.GetTag(TagLib.TagTypes.Id3v2);
		string track = tag.Title;
		string artist = tag.FirstPerformer;

		if (track != null && artist != null) {
			WWW www = new WWW ("http://api.soundcloud.com/tracks?client_id=1960bf2acbdd4e5d8d43b38df11da507&q=" + track + "," + artist);
			yield return www;
			string str = www.text;

			if (str != null) {
				//take the first match and find genre
				string str1 = "\"id\"";
				Regex r = new Regex (str1 + @":(\d*)");
				string id = r.Match (str).Groups [1].Value;
				www = new WWW ("http://api.soundcloud.com/tracks/" + id + ".json?client_id=1960bf2acbdd4e5d8d43b38df11da507");
				yield return www;
				str = www.text;
				str1 = "\"genre\"";
				string str2 = "\"([^\"]*)\"";
				r = new Regex (str1 + ":" + str2);
				genre = genreFromString (r.Match (str).Groups [1].Value);
				Debug.Log (genre);
			}
		}

		yield return null;

	}

	private Genre genreFromString (string str) {
		string lc = str.ToLower ();
		Debug.Log (lc);
		//more specific types first
		if (lc.Contains ("alternative") || lc.Contains ("indie"))
			return Genre.Alternative;
		else if (lc.Contains ("classic") || lc.Contains ("baroque") || lc.Contains ("romantic")
		         || lc.Contains ("orchestra"))
			return Genre.Classical;
		else if (lc.Contains ("metal") || lc.Contains ("scream") || lc.Contains ("core"))
			return Genre.Metal;
		else if (lc.Contains ("reggae"))
			return Genre.Reggae;
		else if (lc.Contains ("rock"))
			return Genre.Rock;
		else if (lc.Contains ("trance"))
			return Genre.Trance;
		else if (lc.Contains ("rap") || lc.Contains ("hip") || lc.Contains ("hiphop"))
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
		else return Genre.Electronic;
		Debug.Log (genre);
	}

}
