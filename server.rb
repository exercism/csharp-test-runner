gem 'sinatra'

require 'json'
require 'puma'
require 'sinatra'
require 'sinatra/json'

post '/job' do
  # Uniq ID for this job
  job_id = SecureRandom.uuid

  # Create dirs
  job_dir = "/tmp/jobs/#{job_id}"
  input_dir = "#{job_dir}/input"
  output_dir = "#{job_dir}/output"
  FileUtils.mkdir_p(input_dir)
  FileUtils.mkdir_p(output_dir)

  # Setup input
  zip_file = "#{job_dir}/files.gzip"
  File.write(zip_file, params[:zipped_files])
  `unzip #{zip_file} -d #{input_dir}`
  logger.info `ls -lah #{input_dir}`

  # Run command
  cmd = "/bin/sh bin/run.sh #{params[:exercise]} #{input_dir} #{output_dir}"
  exit_status = Dir.chdir("/opt/test-runner/") do
    system(cmd)
  end

  # Read and return the result
  begin
    result = File.read("#{output_dir}/#{params[:results_filepath]}")
  rescue Errno::ENOENT
  end

  json(
    exit_status: exit_status,
    result: result
  )
end
