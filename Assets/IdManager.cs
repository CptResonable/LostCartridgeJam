public static class IdManager {
    private static uint idTicker = 0;
    public static uint CreateNewID() {
        idTicker++;
        return idTicker;
    }
}
