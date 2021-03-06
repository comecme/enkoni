#-------------------------------------------------------------------------
# Validates an XML file using a XSD file.
# ------------------------------------------------------------------------
function Validate-ReleaseNotes($xmlPath, $xsdPath) {
    #$xsdPath = -join ((Get-Location), '\', $xsdPath)
    #$xmlPath = -join ((Get-Location), '\', $xmlPath)
    #Define the script block that will act as the validation event handler
    $code = @' 
        param($sender,$a) 
        $ex = $a.Exception 
        #Trim out the useless,irrelevant parts of the message
        $msg = $ex.Message -replace " in namespace 'http.*?'","" 
        #Create the custom error object using a hashtable
        $properties = @{LineNumber=$ex.LineNumber; LinePosition=$ex.LinePosition; Message=$msg} 
        $o = New-Object PSObject -Property $properties 
        #Add the object to the $validationerrors array
        $validationerrors += $o 
'@
    
    #Convert the code block to as ScriptBlock
    $validationEventHandler = [scriptblock]::Create($code) 
    
    #Create a new XmlReaderSettings object
    $rs = New-Object System.Xml.XmlreaderSettings 
    #Load the schema into the XmlReaderSettings object
    [Void]$rs.schemas.add('http://www.oscarbrouwer.nl/enkoni/2012/09',(new-object System.Xml.xmltextreader($xsdPath))) 
    #Instruct the XmlReaderSettings object to use Schema validation
    $rs.validationtype = "Schema" 
    $rs.ConformanceLevel = "Auto" 
    #Add the scriptblock as the ValidationEventHandler
    $rs.add_ValidationEventHandler($validationEventHandler) 
    
    #Create the XmlReader object using the settings defined previously
    $reader = [System.Xml.XmlReader]::Create($xmlPath,$rs) 
    
    #Temporarily set the ErrorActionPreference to SilentlyContinue, 
    #as we want to use our validation event handler to handle errors
    $previousErrorActionPreference = $ErrorActionPreference 
    $ErrorActionPreference = "SilentlyContinue" 
     
    #Read the Xml using the XmlReader
    while ($reader.read()) {$null} 
    #Close the reader 
    $reader.close() 
    
    #Reset the ErrorActionPreference back to the previous value
    $ErrorActionPreference = $previousErrorActionPreference  
     
    #Return the array of validation errors
    return $validationerrors 
}

#-------------------------------------------------------------------------
# Reads the contents of the release notes and puts the information of the
# latest release into an object.
# ------------------------------------------------------------------------
function Read-ReleaseNotes([string]$path) {
  if(-not $path) {
    Write-Error 'Usage: Read-ReleaseNotes -path [path to release notes]'
    return -1
  }
  
  # Read the contents of the xml-file
  [xml]$xml = Get-Content $path
  
  [int]$exitCode = $LastExitCode
  if($exitCode -ne 0) {
    return
  }

  # Define and fill a new object that will hold the release information
  $releaseInfo = "" | Select-Object Version, Projects, Packages
  $release = $xml.enkoni.releases.release[0]
  $releaseInfo.Version = $release.version
  $releaseInfo.Projects = @{}
  $releaseInfo.Packages = @{}
  
  # Loop through the available projects to collect the project information
  foreach($project in $release.projects.project) {
    # Define and fill a new object that will hold the project information
    $projectInfo = "" | Select-Object ProjectName, PackageId, Version, VersionPostfix, Comments
    $projectInfo.ProjectName = $project.name
    $projectInfo.PackageId = $project.name.Replace(".Framework", "")
    $projectInfo.Version = $project.version
	$projectInfo.VersionPostfix = $project.versionPostfix
    
    # Loop through the available updates in order to construct the release notes for the current project
    foreach($update in $project.updates.update) {
      # Read the content, split it at the line end and trim each line to remove unwanted spaces
      $comments = $update.comment.get_InnerText().Split([System.Environment]::NewLine)
      for($i=0; $i -lt $comments.Length; $i++){
        $comments[$i] = $comments[$i].Trim()
      }
      
      # Recombine the lines into a single string
      $tempComments = $comments -join [System.Environment]::NewLine
      
      # Append the comments with the comments of the previous updates (if any)
      if(-not $releaseComments) {
        $releaseComments = $tempComments
      }
      else {
        $releaseComments = $releaseComments, $tempComments -join [System.Environment]::NewLine
      }
    }
    
    $projectInfo.Comments = $releaseComments
    Clear-Variable releaseComments
    
    $releaseInfo.Projects.Add($projectInfo.ProjectName, $projectInfo)
    $releaseInfo.Packages.Add($projectInfo.PackageId, $projectInfo)
  }
  
  return $releaseInfo
}

#-------------------------------------------------------------------------
# Updates the version numbers of each project.
# ------------------------------------------------------------------------
function Update-Versions($releaseInfo) {
  if(-not $releaseInfo) {
    Write-Error 'Usage: Update-Versions -releaseInfo [info regarding the release] -versionPostFix [version postfix]'
    return -1
  }

  # Update the version number in SolutionInfo.cs
  $productVersion = $releaseInfo.Version
  $path = -join ($projectDir, '\Source\SolutionInfo.cs')
  (Get-Content $path) |
    Foreach-Object { $_ -Replace "[1-9]\d*\.(0|([1-9]\d*))\.(0|([1-9]\d*))\.(0|([1-9]\d*))",  $productVersion } |
    Set-Content $path -Encoding UTF8;
  
  # Update the version number for the documentation
  [xml]$shfb = Get-Content (-join ($projectDir, '\Source\Enkoni.Documentation.shfbproj'))
  $shfb.Project.PropertyGroup[0].HelpFileVersion = $productVersion
  $shfb.Save(-join ($projectDir, '\Source\Enkoni.Documentation.shfbproj'))
  Clear-Variable shfb
  
  # Update the version number of each available project
  $releaseInfo.Projects.GetEnumerator() | Foreach-Object {
    $path = -join ($projectDir, "\Source\" ,$_.Name ,"\Properties\AssemblyVersionInfo.cs")
    $assemblyVersion = $_.Value.Version
    
    (Get-Content $path) |
      Foreach-Object { $_ -Replace "`"[1-9]\d*\.(0|([1-9]\d*))\.(0|([1-9]\d*))(\.(0|([1-9]\d*))(\-[a-zA-Z0-9]+))?`"", ("`"" + $assemblyVersion + "`"") } |
      Set-Content $path -Encoding UTF8;
  }
}

#-------------------------------------------------------------------------
# Clears a directory.
# ------------------------------------------------------------------------
function Clear-Directory($path) {
  #Get-ChildItem $path | Where-Object {$_.mode -notmatch "d"} |Select-Object |Remove-Item
  Get-ChildItem $path | Select-Object | Remove-Item -Recurse -Force
}

#-------------------------------------------------------------------------
# Builds the solution using MSBuild
#-------------------------------------------------------------------------
function Build-Solution($solutionFile, $outputPath) {
  $aliasExists = Test-Path Alias:\msbuild
  if(!$aliasExists) {
    Set-Alias msbuild "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
  }
  
  if(-not $outputPath) {
    msbuild /p:Configuration=Release /p:Platform="Any CPU" /l:"FileLogger,Microsoft.Build.Engine`;logfile=Build.log" /t:rebuild $solutionFile
  }
  else {
    msbuild /p:Platform="Any CPU" /p:Configuration=Release`;OutputPath=$outputPath /t:rebuild /logger:"FileLogger,Microsoft.Build.Engine`;logfile=Build.log" $solutionFile
  }
  
  $exitCode = $LastExitCode

  if($exitCode -ne 0) {
    Write-Host 'MSBuild failed. Review MSBuild-output and try again'
    return $exitCode
  }
  
  return 0
}

#-------------------------------------------------------------------------
# Executes the unit tests that are available using MSTest
#-------------------------------------------------------------------------
function Test-Solution() {
  $aliasExists = Test-Path Alias:\mstest
  if(!$aliasExists) {
    Set-Alias mstest "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe"
  }
  
  #Find the test-assemblies
  $testAssemblies = Get-ChildItem .\WorkingDir -Filter Enkoni.Framework*.Tests.dll | Select-Object Name

  $testContainers = ""
  foreach($i in $testAssemblies) { 
    $testContainers = -join ($testContainers, ' /testcontainer:..\WorkingDir\', $i.Name)
  }

  $testContainers = $testContainers.TrimStart()

  #Change the working directory
  Set-Location Source

  $command = -join ('mstest ', $testContainers)
  Write-Host $command
  #Test the solution
  Invoke-Expression $command

  $exitCode = $LastExitCode

  #Change back to the parent directory
  Set-Location ..

  if($exitCode -ne 0) {
    Write-Host 'MSTest failed. Review the test-results and try again'
    return $exitCode
  }
}

#-------------------------------------------------------------------------
# Transforms the release notes XML-file into an easy-to-read text file.
#-------------------------------------------------------------------------
function Write-ReleaseNotes($xml = "ReleaseNotes.xml", $xsl = "ReleaseNotes.xsl", $output = "Release Notes.txt") {
  if(-not $xml -or -not $xsl -or -not $output) {
	Write-Host "& .Write-ReleaseNotes [-xml] xml-input [-xsl] xsl-input [-output] transform-output"
	exit;
  }

  trap [Exception] {
	Write-Host $_.Exception;
  }

  $xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
  $xslt.Load($xsl);
  $xslt.Transform($xml, $output);
  Write-Host "generated" $output
}

#-------------------------------------------------------------------------
# Signs the assemblies and copies them back to the bin\Release folder
#-------------------------------------------------------------------------
function Sign-Assemblies($certificate) {
  if(-not $certificate) {
    $certificate = '.\Source\Enkoni.Framework.pfx'
  }
  
  $timeStamper = "http://timestamp.verisign.com/scripts/timestamp.dll"
  # Read the certificate (will prompt for a password)
  $sign1Time = [System.DateTime]::Now
  $cert = Get-PfxCertificate $certificate
  $sign2Time = [System.DateTime]::Now
  
  # Locate the assemblies that must be signed
  $assemblies = Get-ChildItem .\WorkingDir\Enkoni.Framework*.dll | Select-Object FullName
  # Sign each assembly
  foreach($assembly in $assemblies) {
    Set-AuthenticodeSignature -Filepath $assembly.FullName -Certificate $cert -TimeStampServer $timeStamper
  }
  
  # Copy the signed assemblies (and the pdb's and xml's) to the default release-directory (needed for a successfull NuGet-package)
  $projectDirs = Get-ChildItem '.\Source' | Where-Object {$_.Mode -match "d" -and $_.Name -match "Enkoni.Framework*"} |Select-Object Name
  foreach($projectDir in $projectDirs) {
    $path = -join ('.\Source\', $projectDir.Name, '\bin\Release\')
    Copy-Item -Path '.\WorkingDir\*' -Include (-join ($projectDir.Name, '.dll')), (-join ($projectDir.Name, '.pdb')), (-join ($projectDir.Name, '.xml')) -Destination $path
  }
}

#-------------------------------------------------------------------------
# Updates release notes and dependencies in the the NuSpec-files using
# the supplied release information and creates the actual NuGet packages.
# ------------------------------------------------------------------------
function Pack-NuGet($releaseInfo, $nuGetExecutable, $outputDirectory) {
  if(-not $releaseInfo -or -not $outputDirectory) {
    Write-Error 'Usage: Pack-NuGet -releaseInfo [release information] -outputDirectory [output directory] (-nuGetExecutable [path to NuGet.exe])'
    return -1
  }
  
  if(-not $nuGetExecutable) {
    $nuGetExecutable = '.\Tools\NuGet.exe'
  }
  
  if(-not $outputDirectory) {
    $outputDirectory = '.\'
  }
  
  if(!(Test-Path $nuGetExecutable)) {
    Write-Error $nuGetExecutable could not be located
  }
  
  $aliasExists = Test-Path Alias:\nuget
  if(!$aliasExists) {
    Set-Alias nuget $nuGetExecutable
  }
  
  # Loop through the available projects
  $releaseInfo.Projects.GetEnumerator() | Foreach-Object {
    # Construct the path to the NuSpec-file for this project and read its contents
    $path = -join ($projectDir, ".\Source\" ,$_.Name ,"\", $_.Name, ".nuspec")
    [xml]$nuspec = Get-Content $path
    
    # Set the release notes in the NuSpec file
    $comments = $_.Value.Comments
    $cdata = $nuspec.CreateCDataSection($comments)
    
	$nuGetVersion = $_.Value.Version
	if($_.Value.VersionPostfix -ne $null -and $_.Value.VersionPostfix -ne '') {
		$nuGetVersion = -join ($nuGetVersion, "-", $_.Value.VersionPostfix)
	}
	
    $nuspec.package.metadata["releaseNotes"].InnerXml = $cdata.OuterXml
    $nuspec.package.metadata.id = $_.Value.PackageId
    $nuspec.package.metadata.version = $nuGetVersion
    
    # Loop through the dependencies
    foreach($dependency in $nuspec.package.metadata.dependencies.dependency) {
      # If the dependency denotes an Enkoni-project, update the versionnumber
      if($dependency -and $releaseInfo.Packages.ContainsKey($dependency.id)) {
        $packageVersion = $releaseInfo.Packages[$dependency.id].Version
        if($releaseInfo.Packages[$dependency.id].VersionPostfix -ne $null -and $releaseInfo.Packages[$dependency.id].VersionPostfix -ne '') {
		  $packageVersion = -join ($packageVersion, "-", $releaseInfo.Packages[$dependency.id].VersionPostfix)
	    }
		
		$dependency.version = $packageVersion
      }
    }
    
    # Finally, save the NuSpec...
    $nuspec.Save($path)
    
    # ...and create the package 
    $csproj = -join (".\Source\" ,$_.Name ,"\", $_.Name, ".csproj")
    
    nuget pack $csproj -Prop Configuration=Release -Prop Platform=AnyCPU -OutputDirectory $outputDirectory -Symbols
  }
}

function Push-NuGet($nuGetExecutable, $inputDirectory, $apiKey, $server) {
  if(-not $inputDirectory -or -not $apiKey) {
    Write-Error 'Usage: Push-NuGet -inputDirectory [input directory] -apiKey [API Key] (-nuGetExecutable [path to NuGet.exe])'
    return -1
  }
  
  if(-not $nuGetExecutable) {
    $nuGetExecutable = '.\Tools\NuGet.exe'
  }
  
  if(-not $inputDirectory) {
    $inputDirectory = '.\'
  }
  
  if(-not $server) {
    $server = 'https://www.nuget.org/api/v2/package'
  }
  
  if(!(Test-Path $nuGetExecutable)) {
    Write-Error $nuGetExecutable could not be located
  }
  
  $aliasExists = Test-Path Alias:\nuget
  if(!$aliasExists) {
    Set-Alias nuget $nuGetExecutable
  }
  
  #nuget setApiKey $apiKey -Source $server
  nuget setApiKey $apiKey -Source $server
  
  # Locate the packages that must be published
  $packages = Get-ChildItem $inputDirectory\Enkoni*.*.*.*.nupkg | Select-Object Name
  foreach($package in $packages) {
    $packagePath = [System.IO.Path]::Combine($inputDirectory, $package.Name)
    nuget push $packagePath -Source $server
	#nuget push $packagePath
  }
}

function Out-Zip([string]$filter, [string]$path) {
  if (-not $path.EndsWith('.zip')) {
    $path += '.zip'
  }

  $aliasExists = Test-Path Alias:\zip
  if(!$aliasExists) {
    Set-Alias zip .\Tools\7za.exe  
  }

  zip a $path $filter
}