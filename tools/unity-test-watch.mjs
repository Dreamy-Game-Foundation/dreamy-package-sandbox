const endpoint = "http://127.0.0.1:8080/mcp";
const target = process.argv.find(value => value.startsWith("--target="))?.slice("--target=".length);
const args = process.argv.slice(2).filter(value => !value.startsWith("--target="));
let jobId = args[0];
if (!jobId) throw new Error("Expected an existing Unity MCP test job ID.");

const headers = {
  "Content-Type": "application/json",
  Accept: "application/json, text/event-stream",
};
let sequence = 1;
async function request(method, params) {
  const id = sequence++;
  const response = await fetch(endpoint, {
    method: "POST",
    headers,
    body: JSON.stringify({ jsonrpc: "2.0", id, method, params }),
    signal: AbortSignal.timeout(90000),
  });
  const session = response.headers.get("mcp-session-id");
  if (session) headers["Mcp-Session-Id"] = session;
  const text = await response.text();
  if (!response.ok) throw new Error(`MCP HTTP ${response.status}: ${text}`);
  const messages = text.startsWith("{")
    ? [JSON.parse(text)]
    : text.split("\n").filter(line => line.startsWith("data: "))
      .map(line => JSON.parse(line.slice(6)));
  const message = messages.find(value => value.id === id);
  if (!message) throw new Error(`Missing MCP response for ${method}.`);
  if (message.error) throw new Error(JSON.stringify(message.error));
  return message.result;
}

await request("initialize", {
  protocolVersion: "2024-11-05",
  capabilities: {},
  clientInfo: { name: "dreamy-test-watch", version: "1.0" },
});
if (target) {
  const selected = await request("tools/call", {
    name: "set_active_instance",
    arguments: { instance: target },
  });
  const parsed = selected.structuredContent
    ?? JSON.parse(selected.content.find(value => value.type === "text").text);
  if (!parsed.success) throw new Error(JSON.stringify(parsed));
}
if (jobId === "--economy" || jobId === "--run") {
  const mode = jobId === "--economy" ? "EditMode" : args[1];
  const assemblies = jobId === "--economy"
    ? ["Dreamy.Economy.Runtime.Tests"]
    : args.slice(2);
  if (!["EditMode", "PlayMode"].includes(mode) || assemblies.length === 0)
    throw new Error("Usage: --run EditMode|PlayMode AssemblyName [AssemblyName ...]");
  const started = await request("tools/call", {
    name: "run_tests",
    arguments: {
      mode,
      assembly_names: assemblies,
      include_failed_tests: true,
      init_timeout: 120000,
    },
  });
  const parsed = started.structuredContent?.result ?? started.structuredContent
    ?? JSON.parse(started.content.find(value => value.type === "text").text);
  console.log("TEST_START " + JSON.stringify(parsed));
  if (!parsed.success || !parsed.data?.job_id) throw new Error("No test job started.");
  jobId = parsed.data.job_id;
}
if (jobId === "--refresh") {
  const result = await request("tools/call", {
    name: "refresh_unity",
    arguments: {
      mode: "force",
      scope: "all",
      compile: "request",
      wait_for_ready: true,
    },
  });
  console.log("REFRESH_RESULT " + JSON.stringify(result));
  if (result.isError) process.exitCode = 1;
  process.exit(process.exitCode ?? 0);
}
if (jobId === "--editor") {
  const readyDeadline = Date.now() + 600000;
  while (Date.now() < readyDeadline) {
    const result = await request("resources/read", { uri: "mcpforunity://editor/state" });
    const state = JSON.parse(result.contents[0].text);
    if (state.success && state.data.advice.ready_for_tools) {
      console.log("EDITOR_READY " + JSON.stringify(state.data.compilation));
      process.exit(0);
    }
    // This process is owned by a monitor; no agent turn waits or polls.
    await new Promise(resolve => setTimeout(resolve, 500));
  }
  throw new Error("Unity did not become ready before the monitor deadline.");
}
if (jobId === "--instance") {
  const name = args[1];
  if (!name) throw new Error("Expected the exact Unity project name.");
  const readyDeadline = Date.now() + 600000;
  while (Date.now() < readyDeadline) {
    const result = await request("resources/read", { uri: "mcpforunity://instances" });
    const state = JSON.parse(result.contents[0].text);
    const instance = state.instances?.find(value => value.name === name);
    if (instance) {
      console.log("UNITY_INSTANCE_READY " + JSON.stringify(instance));
      process.exit(0);
    }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  throw new Error(`Unity project ${name} did not connect before the monitor deadline.`);
}
const deadline = Date.now() + 600000;
let previousStatus;
while (Date.now() < deadline) {
  const result = await request("tools/call", {
    name: "get_test_job",
    arguments: {
      job_id: jobId,
      include_failed_tests: true,
      include_details: true,
      wait_timeout: 30,
    },
  });
  const parsed = result.structuredContent?.result
    ?? result.structuredContent
    ?? JSON.parse(result.content.find(value => value.type === "text").text);
  if (!parsed.success) throw new Error(JSON.stringify(parsed));
  const data = parsed.data;
  if (data.status !== previousStatus) {
    console.log(`TEST_JOB_STATUS ${jobId} ${data.status}`);
    previousStatus = data.status;
  }
  if (["running", "queued", "pending"].includes(data.status)) continue;
  console.log(`TEST_JOB_RESULT ${JSON.stringify(parsed)}`);
  process.exitCode = ["failed", "error", "cancelled"].includes(data.status) ? 1 : 0;
  break;
}
if (previousStatus === "running" || previousStatus === "queued" || previousStatus === "pending")
  throw new Error(`Test job ${jobId} exceeded the watch deadline.`);
