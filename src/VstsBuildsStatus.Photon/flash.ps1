particle compile photon
$file = ls -Filter *.bin | select -First 1
particle flash P2 $file.Name
rm $file