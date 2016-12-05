param($device = "P2")

particle compile photon
$file = ls -Filter *.bin | select -First 1
particle flash $device $file.Name
rm $file