#########################################
  Language: In Version 1.52 hinzugefürt
 	Eigene Sprache hinzufügen.
#########################################

Hier lernst du, wie du eine eigene Sprachen zu deinem Programm hinzufügen kannst.

Dateiname:

 - Bei dem Dateinamen sollte daraug geachtet werden, das der name mit "lang_"being.
 - Es werden nur Text (.txt) Datein akzeptiert.
 - Nach "lang_" kommen zwei zeichen, welche die jeweilige Sprach beschriebt zum Beispiel "xx"
 - Groß und klein Schreibung spielt keine Rolle bei des benennung der txt Datei.

	Beispiel der benennung der Text Datei: "lang_xx.txt"

Inhalt:

 - Wichtig hierbei ist es das alle Platzhalter übernommen werden.
 - Sollte ein Platzhalten fehler, wird dieser durch die System integrierte Sprache ersetzt.
 - Andere Datei können zum erstellen einere neuen Sprache als Vorlage benutz werden.
 - Platzhalten dürfen aber nicht verändert werden.
 - Die System sprache ist Englisch, sollte jetzt einen Sprache mit dem Dateiname "lang_en.txt" hinzugefüt werden.
 - Wird die Integrierte System Sprache "English (System)" durch die neue Sprach erstetzt werden.

	Beispiel hierfür -> Language: Deutsch
	
	 - Dabei darf Language nicht verändert werden.
	 - Ansonsten wird der Platzhalter durch die System Sprache ersetzt.+
	 - In dem Fall wäre Deutsch danach English (System)

Icon:

 - In dem Verzeichnis befinden sich Icons.
 - Soll einer Sprache ein Icon zugewiesen werden, muss das wie folgt aus sehen "flag_xx.png"
 - Hier bei ist es Wichtig dass, das Icon eine PNG ist.
 - Ansonsten wird dieses nicht angezeigt, bzw. durch das Icon "flag_world" ersetzt.


Zusammenfassung:

	Neue Sprache ->

		Dateiname: lang_xx.txt (lang_xx)

		Icon: flag_xx.png (flag_xx)

		Inhalt: Beispiel in der Datei -> "lang_en.txt" (lang_en)

