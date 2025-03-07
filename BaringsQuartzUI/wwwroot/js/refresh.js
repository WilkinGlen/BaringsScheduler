function refreshJobRunCounts(dotNetObjectReference) {
    setInterval(function () {
        dotNetObjectReference.invokeMethodAsync('RefreshJobRunCounts');
    }, 10000);
}