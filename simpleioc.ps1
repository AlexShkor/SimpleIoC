Import-Module -Name C:\git\suvoda\source\deployment\posh-git.psm1 -Verbose

$branch = "autodeploy";
$result = git rev-parse --abbrev-ref HEAD
if ($result -ne $branch){
git checkout $branch
}

$result = git merge master
$result 
if ($result -eq "Already up-to-date."){
    Exit
}

if (!(Test-Path ".\version"))
{
$text = "1`r`n0`r`n0`r`n0";
$text | Out-File 'version'
}

$data = Get-Content ".\version"
$data[3] = [int]$data[3] +1
$data[3]

$data | Out-File 'version'

$result = git add "version"
$result


$version = $data[0] + "." + $data[1] + "." + $data[2] + "." + $data[3];
$message = "release " + $version ;

$result = git tag -a ("v" + $version) -m $message

$result

$result = git commit -m $message
$result


$result = git push origin $branch
$result