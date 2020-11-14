$web = New-Object System.Net.WebClient
$str = $web.DownloadString("http://localhost:51303/anfitrion/BuscarNotificacionAnfitrion")
$str