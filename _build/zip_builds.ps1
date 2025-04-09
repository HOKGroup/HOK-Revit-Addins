# Get the path to the main folder
$mainFolder = "C:\Users\dan.siroky\Work\HOK Revit Addins\Releases\0.1.0.28" # replace with your actual folder path

# Loop through each subfolder in the main folder
Get-ChildItem -Path $mainFolder -Directory | ForEach-Object {
  # Navigate into the subfolder
  Set-Location $_.FullName

  # Get the name of the subfolder (will be used as the zip file name)
  $subfolderName = $_.Name

  # Create a zip file with the contents of the subfolder and its name
  Compress-Archive -Path * -DestinationPath "$($mainFolder)\$($subfolderName).zip"
}