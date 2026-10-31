<?php
$eazypay = 'https://eazypay.icicibank.com/EazyPG';
$qs = isset($_SERVER['QUERY_STRING']) ? $_SERVER['QUERY_STRING'] : '';
if ($qs === '' || strpos($qs, 'merchantid=') === false)
{
    header('HTTP/1.1 400 Bad Request');
    echo 'Missing eazypay query.';
    exit;
}
$pairs = array();
foreach (explode('&', $qs) as $pair)
{
    $parts = explode('=', $pair, 2);
    $k = rawurldecode($parts[0]);
    $v = isset($parts[1]) ? rawurldecode($parts[1]) : '';
    $pairs[] = array($k, $v);
}
header('Referrer-Policy: unsafe-url');
echo '<!DOCTYPE html><html><head><meta charset="utf-8"><meta name="referrer" content="unsafe-url"><title>ICICI eazypay</title></head><body>';
echo '<form id="eazypayForm" method="get" action="' . htmlspecialchars($eazypay, ENT_QUOTES, 'UTF-8') . '" referrerpolicy="unsafe-url">';
foreach ($pairs as $pv)
{
    echo '<input type="hidden" name="' . htmlspecialchars($pv[0], ENT_QUOTES, 'UTF-8') . '" value="' . htmlspecialchars($pv[1], ENT_QUOTES, 'UTF-8') . '" />';
}
echo '</form><script>document.getElementById("eazypayForm").submit();</script></body></html>';
exit;
