
$web = New-Object System.Net.WebClient
$str = $web.DownloadString("http://localhost:51304/registroReferencias/recalcularClasificacionABC")
$str