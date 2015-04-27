using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

public class SongGenre : MonoBehaviour {
	public string genre;
	/*
	
	//getting artist and title thanks to David Peng
	public IEnumerator Request (string path) {
		//get artist and track title
		TagLib.File trk = TagLib.File.Create (path);
		TagLib.Tag tag = trk.GetTag(TagLib.TagTypes.Id3v2);

		string track = tag.Title;
		string artist = tag.FirstPerformer;

		if (artist != null && track != null) {
			WWW www = new WWW ("http://api.soundcloud.com/tracks?client_id=1960bf2acbdd4e5d8d43b38df11da507&q=" + track + "," + artist);
			yield return www;
			string str = www.text;

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
			genre = r.Match (str).Groups [1].Value;
			yield return null;
		}
	}
	*/
}
